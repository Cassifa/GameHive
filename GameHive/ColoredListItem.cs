/*************************************************************************************
 * 文 件 名:   ColoredListItem.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/6 7:44
*************************************************************************************/
namespace GameHive {
    public class ColoredListItem {
        public string Text { get; }
        public Color TextColor { get; }

        public ColoredListItem(string text, Color textColor) {
            Text = text;
            TextColor = textColor;
        }

        public override string ToString() => Text; // 显示文字内容
    }
}
