using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAS1000Converter
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("=====================================");
			Console.WriteLine("LAS-1000 image file to TIFF format converter");
			Console.WriteLine("Copyright 2017 Genta Ito");
			Console.WriteLine("Version 1.1");
			Console.WriteLine("今後の予定");
			Console.WriteLine("V1.2->上書き確認");
			Console.WriteLine("V1.2->飽和ピクセルの存在をファイル名で指摘");
			Console.WriteLine("=====================================");
			Console.WriteLine("【注意】ファイル名にピリオドが含まれるとうまく処理されません");
			Console.WriteLine("=====================================");
			Console.WriteLine("");

			FileStream myFS;
			bool flagPos = false;
			int pos = 0;
			int oldPos = 0;

			// ドラッグアンドドロップされたファイルのファイルパスを取得
			// 先頭に格納される実行ファイル名を除く
			string[] filePath = Environment.GetCommandLineArgs();
			int startIndex = 0;
			int numImg = 0;
			for (int i = 0; i < filePath.Length; i++)
			{
				int len = filePath[i].Length;
				if (filePath[i].Substring(len - 3, 3) != "exe")
				{
					startIndex = i;
					break;
				}
			}
			numImg = filePath.Length - startIndex;
			string[] imageFilePath = new string[numImg];
			Array.Copy(filePath, startIndex, imageFilePath, 0, numImg);

			// ファイルをひとつずつ処理する
			for (int i = 0; i < imageFilePath.Length; i++)
			{
				System.IO.FileInfo info = new System.IO.FileInfo(imageFilePath[i]);
				Console.WriteLine("Processing " + imageFilePath[i]);

				if (!flagPos)
				{
					Console.WriteLine("    Input position [1-7]: ");
					pos = int.Parse(Console.ReadLine());
					oldPos = pos;
				}
				else
				{
					pos = oldPos;
				}

				if (numImg > 1 && !flagPos)
				{
					Console.WriteLine("All the other images taken at the same position? [y/n]");
					var posAns = Console.ReadLine();
					if (posAns == "y")
					{
						flagPos = true;
						Console.WriteLine("The rest of images will be processed automatically.");
					}
					else flagPos = false;
				}

				string fileName = Path.GetFileName(imageFilePath[i]);
				string dirName = Path.GetDirectoryName(imageFilePath[i]);
				string tiffName = Path.GetFileNameWithoutExtension(imageFilePath[i]) + ".tif";
				string tiffPath = dirName + "\\"+ tiffName;

				// イメージファイルをバイナリ変数に格納する
				// バイナリデータ格納用変数
				byte[] raw = new byte[1384 * 922 * 2];
				// BinaryReaderクラスでファイルストリームを読み込むためのオブジェクト作成
				using (BinaryReader br = new BinaryReader(File.Open(imageFilePath[i], FileMode.Open)))
				{
					for (int j = 0; j < raw.Length; j++)
					{
						raw[j] = br.ReadByte();
					}
				}
				
				using (myFS = new FileStream(tiffPath, FileMode.OpenOrCreate, FileAccess.Write))
				{
					using (BinaryWriter bw = new BinaryWriter(myFS))
					{
						myFS.Seek(0x00000000, SeekOrigin.Begin);

						//TIFFファイルの共通ヘッダー
						//タグの数は11個
						byte[] data = { 0x4D, 0x4D, 0x00, 0x2A, 0x00, 0x00, 0x00, 0x08, 0x00, 0x0B };
						bw.Write(data);

						//1個めのタグ
						//ImageWidthタグ（0100）=1384
						byte[] tagImageWidth = { 0x01, 0x00,
									   0x00, 0x03,
									   0x00, 0x00, 0x00, 0x01,
									   0x05, 0x68,
									   0x00, 0x00 };
						bw.Write(tagImageWidth);

						//2個めのタグ
						//ImageHeightタグ =922
						byte[] tagImageHeight = { 0x01, 0x01,
										0x00, 0x03,
										0x00, 0x00, 0x00, 0x01,
										0x03, 0x9A,
										0x00, 0x00 };
						bw.Write(tagImageHeight);

						//3個めのタグ
						//BitsPerSampleタグ=16
						byte[] tagBitsPerSample = { 0x01, 0x02,
										  0x00, 0x03,
										  0x00, 0x00, 0x00, 0x01,
										  0x00, 0x10,
										  0x00, 0x00 };
						bw.Write(tagBitsPerSample);

						//4個めのタグ
						//Compressionタグ=非圧縮
						byte[] tagCompression = { 0x01, 0x03,
										0x00, 0x03,
										0x00, 0x00, 0x00, 0x01,
										0x00, 0x01,
										0x00, 0x00 };
						bw.Write(tagCompression);

						//5個めのタグ
						//PhotometricInterpretationタグ=黒コードモノクロ
						byte[] tagPhotometricInterpretation = { 0x01, 0x06,
													  0x00, 0x03,
													  0x00, 0x00, 0x00, 0x01,
													  0x00, 0x00,
													  0x00, 0x00 };
						bw.Write(tagPhotometricInterpretation);

						//6個めのタグ
						//StripOffsets=0x000000AC
						byte[] tagStripOffsets = { 0x01, 0x11,
										 0x00, 0x04,
										 0x00, 0x00, 0x00, 0x01,
										 0x00, 0x00, 0x00, 0xAC };
						bw.Write(tagStripOffsets);

						//7個めのタグ
						//RowsPerStrip=922
						byte[] tagRowsPerStrip = { 0x01, 0x16,
										 0x00, 0x04,
										 0x00, 0x00, 0x00, 0x01,
										 0x00, 0x00, 0x03, 0x9A };
						bw.Write(tagRowsPerStrip);

						//8個めのタグ
						//StripByteCounts=2552096
						byte[] tagStripByteCounts = { 0x01, 0x17,
											0x00, 0x04,
											0x00, 0x00, 0x00, 0x01,
											0x00, 0x26, 0xF1, 0x20 };
						bw.Write(tagStripByteCounts);

						//9個めのタグ
						//XResolution
						byte[] tagXResolution = { 0x01, 0x1A,
										0x00, 0x05,
										0x00, 0x00, 0x00, 0x01,
										0x00, 0x00, 0x00, 0x92 };
						bw.Write(tagXResolution);

						//10個めのタグ
						//YResolution
						byte[] tagYResolution = { 0x01, 0x1B,
										0x00, 0x05,
										0x00, 0x00, 0x00, 0x01,
										0x00, 0x00, 0x00, 0x9A };
						bw.Write(tagYResolution);

						//11個めのタグ
						//ResolutionUnit=インチ
						byte[] tagResolutionUnit = { 0x01, 0x28,
										   0x00, 0x03,
										   0x00, 0x00, 0x00, 0x01,
										   0x00, 0x02,
										   0x00, 0x00 };
						bw.Write(tagResolutionUnit);

						//ColorMapタグはデフォルト値（なし）のため省略

						//NextIFDOffset
						byte[] NextIFDOffset = { 0x00, 0x00, 0x00, 0x00 };
						bw.Write(NextIFDOffset);

						//XResolution
						byte[] XResolutionNumerator = { 0x00, 0x15, 0x1E, 0x40 };
						bw.Write(XResolutionNumerator);

						byte[] XResolutionDenominator;
						switch (pos)
						{
							case 1:
								XResolutionDenominator = new byte[] { 0x00, 0x00, 0x11, 0x74 };
								break;
							case 2:
								XResolutionDenominator = new byte[] { 0x00, 0x00, 0x17, 0x33 };
								break;
							case 3:
								XResolutionDenominator = new byte[] { 0x00, 0x00, 0x1C, 0x85 };
								break;
							case 4:
								XResolutionDenominator = new byte[] { 0x00, 0x00, 0x22, 0x0E };
								break;
							case 5:
								XResolutionDenominator = new byte[] { 0x00, 0x00, 0x29, 0xB8 };
								break;
							case 6:
								XResolutionDenominator = new byte[] { 0x00, 0x00, 0x31, 0x61 };
								break;
							case 7:
								XResolutionDenominator = new byte[] { 0x00, 0x00, 0x3C, 0x3C };
								break;
							default:
								XResolutionDenominator = new byte[] { 0x00, 0x00, 0x00, 0x00 };
								break;
						}
						bw.Write(XResolutionDenominator);

						//YResolution
						byte[] YResolutionNumerator = { 0x00, 0x0E, 0x11, 0x90 };
						bw.Write(YResolutionNumerator);

						byte[] YResolutionDenominator;
						switch (pos)
						{
							case 1:
								YResolutionDenominator = new byte[] { 0x00, 0x00, 0x0B, 0xA0 };
								break;
							case 2:
								YResolutionDenominator = new byte[] { 0x00, 0x00, 0x0F, 0x74 };
								break;
							case 3:
								YResolutionDenominator = new byte[] { 0x00, 0x00, 0x12, 0xF6 };
								break;
							case 4:
								YResolutionDenominator = new byte[] { 0x00, 0x00, 0x16, 0xB0 };
								break;
							case 5:
								YResolutionDenominator = new byte[] { 0x00, 0x00, 0x1B, 0xCB };
								break;
							case 6:
								YResolutionDenominator = new byte[] { 0x00, 0x00, 0x20, 0xE5 };
								break;
							case 7:
								YResolutionDenominator = new byte[] { 0x00, 0x00, 0x28, 0x21 };
								break;
							default:
								YResolutionDenominator = new byte[] { 0x00, 0x00, 0x00, 0x00 };
								break;
						}
						bw.Write(YResolutionDenominator);
						
						myFS.Seek(0x000000AC, SeekOrigin.Begin);
						bw.Write(raw);
					}
				}

				Console.WriteLine("Saved " + tiffPath);
				Console.WriteLine();

				oldPos = pos;
			}

			// コンソールウィンドウを維持
			Console.WriteLine();
			Console.WriteLine("Finished processing " + numImg.ToString() + " files");
			Console.WriteLine("Hit any key to quit...");
			Console.ReadKey();
		}
	}
}
