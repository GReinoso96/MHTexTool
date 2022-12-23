using System;
using System.IO;

namespace MHTexTool
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine("Extracting: MHTexTool.exe -e [input file.tex.unpacked] [Output.txt]\n" +
					"Rebuilding: MHTexTool.exe -r [filenames.txt] [output file.bin]");
			}
			else
			{
				switch (args[0])
				{
					case "-e":
						ExtractTex(args[1], args[2]);
						Console.WriteLine("Success");
						break;
					case "-r":
						RebuildTex(args[1], args[2]);
						Console.WriteLine("Success");
						break;
				}
			}
		}

		public static void RebuildTex(string fileName,string outName)
		{
			int counter = 0;
			string ln;
			using (StreamReader file = new StreamReader(fileName))
			{
				while ((ln = file.ReadLine()) != null)
				{
					counter++;
				}
			}
			TexFile[] apxFiles = new TexFile[counter];
			string[] inputFiles = new string[counter];
			using (StreamReader file = new StreamReader(fileName))
			{
				for (int i = 0; i < inputFiles.Length; i++)
				{
					inputFiles[i] = file.ReadLine();
					Console.WriteLine($"Reading Line {i} {inputFiles[i]}");
				}
			}

			using(FileStream fstrm = new FileStream(outName, FileMode.Create, FileAccess.ReadWrite))
			{
				BinaryWriter bw = new BinaryWriter(fstrm);
				bw.Write((UInt32)counter);
				for (int i = 0; i < counter; i++)
				{
					Console.WriteLine($"Loading {i} {inputFiles[i]}");
					using(FileStream datastrm = new FileStream(inputFiles[i], FileMode.Open, FileAccess.Read))
					{
						MemoryStream mstrm = new MemoryStream();
						datastrm.CopyTo(mstrm);
						byte[] data = mstrm.ToArray();
						TexFile tfile = new TexFile();
						tfile.ImageData = data;
						tfile.ImageOffset = 0x00000000;
						tfile.ImageSize = 0x00000000;
						apxFiles[i] = tfile;
						//apxFiles[i].ImageData = data;
					}
					apxFiles[i].ImageSize = (uint)apxFiles[i].ImageData.Length;
					bw.Write(0x000000);
					bw.Write((UInt32)apxFiles[i].ImageSize);
				}
				for (int i = 0; i< inputFiles.Length; i++)
                {
					apxFiles[i].ImageOffset = (uint)bw.BaseStream.Position;
					bw.Write(apxFiles[i].ImageData);
				}
				bw.BaseStream.Seek(0x00000004, SeekOrigin.Begin);
				for (int i = 0; i < inputFiles.Length; i++)
				{
					bw.Write((UInt32)apxFiles[i].ImageOffset);
					bw.BaseStream.Seek(0x00000004, SeekOrigin.Current);
				}
				bw.Dispose();
			}
		}

		public static void ExtractTex(string fileName, string outText)
		{
			using(FileStream fstream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				BinaryReader br = new BinaryReader(fstream);
				var textures = br.ReadUInt32();

				TexFile[] texfile = new TexFile[textures];
				string fnStr = "";

				for(int i = 0; i < texfile.Length; i++)
				{
					TexFile tex = new TexFile();
					tex.TexCount = (uint)i;
					tex.ImageOffset = br.ReadUInt32();
					tex.ImageSize = br.ReadUInt32();
					texfile[i] = tex;
					fnStr += $"{fileName}_{i}.apx\n";
				}

				fnStr = fnStr.TrimEnd();
				//fnStr = fnStr.Remove(fnStr.Length - 1);

				for (int i = 0; i < texfile.Length; i++)
				{
					br.BaseStream.Seek(texfile[i].ImageOffset, SeekOrigin.Begin);
					byte[] imageData = new byte[texfile[i].ImageSize];
					imageData = br.ReadBytes((int)texfile[i].ImageSize);
					using (FileStream fout = new FileStream($"{fileName}_{i}.apx", FileMode.Create, FileAccess.ReadWrite))
					{
						fout.Write(imageData, 0, imageData.Length);
						try
						{
							File.WriteAllText($"{outText}", fnStr);
						}
						catch(Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
					}
				}
				br.Dispose();
			}
		}
	}
}
