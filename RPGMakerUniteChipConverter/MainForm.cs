using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPGMakerUniteChipConverter
{
    public partial class MainForm : Form
    {

        // ファイル名を保持するリスト
        List<string> selectedFiles = new List<string>();

        const int CellSize_With_1px = 98;
        const int CellSize = 96;

        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonAddItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PNG files (*.png)|*.png", // pngファイルのみ選択できるようにフィルタを設定
                Multiselect = true // 複数のファイルを選択できるようにする
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {

                // 選択された各ファイルパスについて、すでにリストに存在しない場合のみ追加
                foreach (string filePath in openFileDialog.FileNames)
                {
                    if (!selectedFiles.Contains(filePath))
                    {
                        selectedFiles.Add(filePath);
                    }
                }

                // ListBoxの内容を更新
                UpdateImageListBox();
            }
        }

        private void UpdateImageListBox()
        {
            listBoxImages.Items.Clear(); // ListBoxをクリア

            foreach (string filePath in selectedFiles)
            {
                string fileName = Path.GetFileName(filePath); // ファイルパスからファイル名を取得
                listBoxImages.Items.Add(fileName); // ListBoxにファイル名を追加
            }

            // プレビューを更新
            listBoxImages_SelectedIndexChanged(listBoxImages, EventArgs.Empty);
        }

        // 重複削除
        private void checkBoxhasDuplicates_CheckedChanged(object sender, EventArgs e)
        {
            List<string> newList = new List<string>(); // 重複がない新しいリストを作成

            foreach (string item in selectedFiles)
            {
                if (!newList.Contains(item)) // 既にリストに存在するかどうかを確認
                {
                    newList.Add(item); // まだリストに存在しなければ追加
                }
            }

            selectedFiles = newList; // 既存のリストを新しいリストで置換

            // ListBoxの内容を更新
            UpdateImageListBox();
        }

        private void buttonDeleteItem_Click(object sender, EventArgs e)
        {
            // リストボックスで選択されているアイテムのインデックスを取得
            int selectedIndex = listBoxImages.SelectedIndex;

            if (selectedIndex != -1) // アイテムが選択されている場合
            {
                // リストボックスから選択されているアイテムを削除
                listBoxImages.Items.RemoveAt(selectedIndex);

                // 同じインデックスのアイテムをselectedFilesリストからも削除
                selectedFiles.RemoveAt(selectedIndex);

                UpdateImageListBox();
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            // selectedFilesリストをクリア
            selectedFiles.Clear();

            UpdateImageListBox();
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            // 選択されているアイテムのインデックスを取得
            int selectedIndex = listBoxImages.SelectedIndex;

            // 選択されているアイテムが存在し、かつ最初のアイテムではない場合
            if (selectedIndex > 0)
            {
                // 選択されているアイテムとその前のアイテムを入れ替える
                string selectedFile = selectedFiles[selectedIndex];
                selectedFiles[selectedIndex] = selectedFiles[selectedIndex - 1];
                selectedFiles[selectedIndex - 1] = selectedFile;

                // ListBoxの表示を更新
                UpdateImageListBox();

                // 同じアイテムを選択したままにする（位置が上に移動する）
                listBoxImages.SelectedIndex = selectedIndex - 1;
            }
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            // 選択されているアイテムのインデックスを取得
            int selectedIndex = listBoxImages.SelectedIndex;

            // 選択されているアイテムが存在し、かつ最後のアイテムではない場合
            if (selectedIndex < listBoxImages.Items.Count - 1 && selectedIndex >= 0)
            {
                // 選択されているアイテムとその次のアイテムを入れ替える
                string selectedFile = selectedFiles[selectedIndex];
                selectedFiles[selectedIndex] = selectedFiles[selectedIndex + 1];
                selectedFiles[selectedIndex + 1] = selectedFile;

                // ListBoxの表示を更新
                UpdateImageListBox();

                // 同じアイテムを選択したままにする（位置が下に移動する）
                listBoxImages.SelectedIndex = selectedIndex + 1;
            }
        }

        private void listBoxImages_DragEnter(object sender, DragEventArgs e)
        {
            // ドラッグされたデータがファイルであれば、ドロップを許可
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void listBoxImages_DragDrop(object sender, DragEventArgs e)
        {
            // ドロップされたファイルのパスを取得
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // ファイルがPNGで、かつまだリストに存在しなければ追加
            foreach (string file in files)
            {
                if (Path.GetExtension(file).ToLower() == ".png" && !selectedFiles.Contains(file))
                {
                    selectedFiles.Add(file);
                }
            }

            // ListBoxの内容を更新
            UpdateImageListBox();
        }

        private void listBoxImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedFiles.Count <= 0)
            {
                pictureBoxPreview.Image = null;
                return;
            }

            // 選択されているアイテムのインデックスを取得
            int selectedIndex = listBoxImages.SelectedIndex;

            // インデックスが有効な範囲内であれば、そのアイテムのファイルを読み込み、pictureBoxPreviewに表示
            if (selectedIndex >= 0 && selectedIndex < selectedFiles.Count)
            {
                pictureBoxPreview.Image = Image.FromFile(selectedFiles[selectedIndex]);
            }
            else
            {
                // インデックスが無効な場合は、pictureBoxPreviewをクリア
                pictureBoxPreview.Image = null;
            }
        }

        private void buttonOutput_Click(object sender, EventArgs e)
        {
            if (selectedFiles.Count <= 0)
            {
                return;
            }

            // 単独のファイルパスをリストに包んで渡す

            int cellSize = (radioButtonDeletePx.Checked) ? CellSize_With_1px : CellSize;

            string result = CheckImagesDimensionDivisibility(selectedFiles, radioButtonDeletePx.Checked);
            if (result != null)
            {
                // 問題が見つかった場合、メッセージボックスなどで結果を表示
                MessageBox.Show($"{result} のサイズが{cellSize}ピクセルで割り切れません。Unite規格ではありません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ConvartImagesAndSave(selectedFiles);

        }

        private void ConvartImagesAndSave(List<string> filePaths)
        {
            try
            {
                if (radioButtonDeletePx.Checked)
                {
                    TrimImages(filePaths);
                }
                else
                {
                    ExpandImages(filePaths);
                }
                MessageBox.Show("変換が完了しました", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            (Exception ex)
            {
                MessageBox.Show($"変換が失敗しました {ex}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap TrimImage(string filePath)
        {
            using (Bitmap originalBitmap = new Bitmap(filePath))
            {
                // トリミング後の各セクションのサイズ
                int sectionSize = 96;

                // 元の画像からセクションの数を計算
                int numSectionsX = (originalBitmap.Width) / CellSize_With_1px;  // 横のセクション数
                int numSectionsY = (originalBitmap.Height) / CellSize_With_1px; // 縦のセクション数

                // 新しいBitmapのサイズを計算
                Bitmap newBitmap = new Bitmap(numSectionsX * sectionSize, numSectionsY * sectionSize);

                using (Graphics g = Graphics.FromImage(newBitmap))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                    // 元の画像から新しい画像へのセクションのマッピング
                    for (int y = 0; y < numSectionsY; y++)
                    {
                        for (int x = 0; x < numSectionsX; x++)
                        {
                            // 元のセクションの位置とサイズ（外周の2ピクセルをカット）
                            Rectangle srcRect = new Rectangle(
                                1 + x * CellSize_With_1px,  // 左のマージンを考慮
                                1 + y * CellSize_With_1px,  // 上のマージンを考慮
                                sectionSize,
                                sectionSize
                            );

                            // 新しい画像の対応する位置
                            Rectangle destRect = new Rectangle(
                                x * sectionSize,
                                y * sectionSize,
                                sectionSize,
                                sectionSize
                            );

                            // 画像を描画
                            g.DrawImage(originalBitmap, destRect, srcRect, GraphicsUnit.Pixel);
                        }
                    }
                }

                return newBitmap;
            }
        }

        private Bitmap ExpandImage(string filePath)
        {
            using (Bitmap originalBitmap = new Bitmap(filePath))
            {
                // セクションのサイズを指定
                int sectionSize = CellSize;

                // 元の画像からセクションの数を計算
                int numSectionsX = originalBitmap.Width / sectionSize;
                int numSectionsY = originalBitmap.Height / sectionSize;

                // 新しいBitmapのサイズを計算（1ピクセルの外周を追加）
                Bitmap newBitmap = new Bitmap((numSectionsX * (sectionSize + 2)), (numSectionsY * (sectionSize + 2)));

                using (Graphics g = Graphics.FromImage(newBitmap))
                {
                    g.Clear(Color.Transparent); // 新しい画像を透明で初期化
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                    // 元の画像から新しい画像へのセクションのマッピング
                    for (int y = 0; y < numSectionsY; y++)
                    {
                        for (int x = 0; x < numSectionsX; x++)
                        {
                            // 元のセクションの位置とサイズ
                            Rectangle srcRect = new Rectangle(
                                x * sectionSize,
                                y * sectionSize,
                                sectionSize,
                                sectionSize
                            );

                            // 新しい画像の対応する位置（外周を追加）
                            Rectangle destRect = new Rectangle(
                                (x * (sectionSize + 2)) + 1,
                                (y * (sectionSize + 2)) + 1,
                                sectionSize,
                                sectionSize
                            );

                            // 画像を描画
                            g.DrawImage(originalBitmap, destRect, srcRect, GraphicsUnit.Pixel);
                        }
                    }
                }

                return newBitmap;
            }
        }

        private void TrimImages(List<string> filePaths)
        {
            if (filePaths.Count <= 0)
            {
                return;
            }

            string saveDirectory = Path.Combine(Path.GetDirectoryName(filePaths[0]), "Trimmed_unite_files");
            Directory.CreateDirectory(saveDirectory);

            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                string savePath = Path.Combine(saveDirectory, fileName);

                Bitmap newBitmap = TrimImage(filePath);
                newBitmap.Save(savePath, ImageFormat.Png);
                newBitmap.Dispose();
            }
        }

        private void ExpandImages(List<string> filePaths)
        {
            if (filePaths.Count <= 0)
            {
                return;
            }

            string saveDirectory = Path.Combine(Path.GetDirectoryName(filePaths[0]), "Converted_unite_files");
            Directory.CreateDirectory(saveDirectory);

            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                string savePath = Path.Combine(saveDirectory, fileName);

                Bitmap newBitmap = ExpandImage(filePath);
                newBitmap.Save(savePath, ImageFormat.Png);
                newBitmap.Dispose();
            }
        }


        private void ShowPreview(string filePath)
        {

            var filePaths = new List<string> { filePath };

            int cellSize = (radioButtonDeletePx.Checked) ? CellSize_With_1px : CellSize;

            string result = CheckImagesDimensionDivisibility(filePaths, radioButtonDeletePx.Checked);
            if(result != null)
            {
                // 問題が見つかった場合、メッセージボックスなどで結果を表示
                MessageBox.Show($"{result} のサイズが{cellSize}ピクセルで割り切れません。Unite規格ではありません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Bitmap newBitmap;

            if (radioButtonDeletePx.Checked)
            {
                newBitmap = TrimImage(filePath);
            }
            else
            {
                newBitmap = ExpandImage(filePath);
            }

            // PictureBoxに表示する
            if (pictureBoxPreview.InvokeRequired)
            {
                pictureBoxPreview.Invoke(new Action(() => pictureBoxPreview.Image = newBitmap));
            }
            else
            {
                pictureBoxPreview.Image = newBitmap;
            }

        }

        private string CheckIfAllImagesHaveSameWidth(List<string> filePaths)
        {
            if (filePaths.Count <= 0)
            {
                return null;
            }

            // 最初の画像の幅を取得
            using (Bitmap firstBitmap = new Bitmap(filePaths[0]))
            {
                int firstImageWidth = firstBitmap.Width;

                // 他のすべての画像が同じ幅を持つか確認
                foreach (string filePath in filePaths.Skip(1))
                {
                    using (Bitmap bitmap = new Bitmap(filePath))
                    {
                        if (bitmap.Width != firstImageWidth)
                        {
                            // 幅が異なる画像が見つかった場合は、そのファイル名を返す
                            return filePath;
                        }
                    }
                }
            }

            // すべての画像が同じ幅を持つ場合は、nullを返す
            return null;
        }

        private string CheckIfAllImagesHaveSameDpi(List<string> filePaths)
        {
            if (filePaths.Count <= 0)
            {
                return null;
            }

            // 最初の画像のDPIを取得して四捨五入
            using (Bitmap firstBitmap = new Bitmap(filePaths[0]))
            {
                int firstImageHorizontalDpi = (int)Math.Round(firstBitmap.HorizontalResolution);
                int firstImageVerticalDpi = (int)Math.Round(firstBitmap.VerticalResolution);

                // 他のすべての画像が同じDPI（四捨五入後の整数部分）を持つか確認
                foreach (string filePath in filePaths.Skip(1))
                {
                    using (Bitmap bitmap = new Bitmap(filePath))
                    {
                        if ((int)Math.Round(bitmap.HorizontalResolution) != firstImageHorizontalDpi ||
                            (int)Math.Round(bitmap.VerticalResolution) != firstImageVerticalDpi)
                        {
                            // DPI（四捨五入後の整数部分）が異なる画像が見つかった場合は、そのファイル名を返す
                            return filePath;
                        }
                    }
                }
            }

            // すべての画像が同じDPI（四捨五入後の整数部分）を持つ場合は、nullを返す
            return null;
        }

        private string CheckImagesDimensionDivisibility(List<string> filePaths,bool With1Px = true)
        {
            if (filePaths.Count <= 0)
            {
                // ファイルリストが空の場合はnullを返す
                return null;
            }

            int cellSize = (With1Px) ? CellSize_With_1px : CellSize;

            foreach (string filePath in filePaths)
            {
                using (Bitmap bitmap = new Bitmap(filePath))
                {
                    if (bitmap.Width % cellSize != 0 || bitmap.Height % cellSize != 0)
                    {
                        // 幅または高さがcellSizeで割り切れない画像が見つかった場合は、そのファイルパスを返す
                        return filePath;
                    }
                }
            }

            // すべての画像の幅と高さが98で割り切れる場合はnullを返す
            return null;
        }

        private void radioButtonPrevColor1_CheckedChanged(object sender, EventArgs e)
        {
            ChangePreviewBackgroundColor();
        }

        private void radioButtonPrevColor2_CheckedChanged(object sender, EventArgs e)
        {
            ChangePreviewBackgroundColor();
        }

        private void ChangePreviewBackgroundColor()
        {
            if (radioButtonPrevColor1.Checked)
            {
                panelImagePreview.BackColor = Color.Black;
            }
            else
            {
                panelImagePreview.BackColor = SystemColors.Control;
            }
        }

        private void buttonPreviewOutput_Click(object sender, EventArgs e)
        {
            if (listBoxImages.SelectedIndex == -1)
                return;

            if (selectedFiles.Count <= 0)
            {
                return;
            }

            ShowPreview(selectedFiles[listBoxImages.SelectedIndex]);
        }


    }
}
