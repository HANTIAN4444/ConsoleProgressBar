/* File Comments
 *****************************************************************************
 * File        : ConsoleProgressBar.cs
 * Description : ����̨������
 * Version     : 1.0
 * Created     : 2020/02/23
 * Author      : Hant
 ****************************************************************************/

using System;
using System.Text;

namespace Hant.Helper
{
    /// <summary>
    /// ����̨������
    /// </summary>
    public class ConsoleProgressBar
    {
        /// <summary>
        /// ����������ʽ��ģ��
        /// </summary>
        private const string DESC_FORMAT = "\0{0,4:F1}%\n";

        /// <summary>
        /// �����������ݵĳ���
        /// </summary>
        private const int DESC_LENGTH = 6;

        /// <summary>
        /// ͬ����
        /// </summary>
        private static readonly object locker = new object();

        private string title;
        private char blockContent;
        private int blockWidth;
        private double minValue;
        private double maxValue;
        private int barLeft;
        private int barTop;
        private int blockStartIndex;
        private double blockSingleModulus;
        private int descStartIndex;
        private char[] output;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="title">����������</param>
        /// <param name="blockContent">���ȷ�����</param>
        /// <param name="blockWidth">���ȷ�����Ŀ��</param>
        /// <param name="minValue">��С����ֵ</param>
        /// <param name="maxValue">������ֵ</param>
        public ConsoleProgressBar(
            string title = "",
            char blockContent = '*',
            int blockWidth = 32,
            double minValue = 0d,
            double maxValue = 100d)
        {
            this.title = String.Empty.Equals(title) ? title : $"{title}:\0";
            this.blockContent = blockContent;
            this.blockWidth = blockWidth;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.blockSingleModulus = ((maxValue - minValue) / blockWidth);

            this.barLeft = Console.CursorLeft;
            this.barTop = Console.CursorTop;
            this.blockStartIndex = this.title.Length + 1;
            this.descStartIndex = this.title.Length + this.blockWidth + 2;

            this.output = CreateOutput(
                this.title,
                this.blockWidth);
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="title">����������</param>
        /// <param name="blockWidth">���ȷ�����Ŀ��</param>
        /// <returns>�������</returns>
        private char[] CreateOutput(
            in string title,
            in int blockWidth)
        {
            var contentLength = title.Length + 2 + blockWidth + DESC_LENGTH;
            var strBuilder = new StringBuilder(contentLength);

            strBuilder.Append(title);
            strBuilder.Append('[');

            for (int i = 0; i < blockWidth; i++)
            {
                strBuilder.Append('\0');
            }

            strBuilder.Append(']');
            strBuilder.Append(String.Format(DESC_FORMAT, 0));

            return strBuilder.ToString().ToCharArray();
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="output">�������</param>
        /// <param name="blockStartIndex">���ȷ���ʼ����</param>
        /// <param name="singleBlockProgress">��һ���ȷ���ʾ�Ľ���</param>
        /// <param name="fillModulus">���ϵ��</param>
        /// <returns>���º���������</returns>
        private char[] UpdateOutput(
            in char[] output,
            in int blockWidth,
            in int blockStartIndex,
            in char blockContent,
            in double singleBlockProgress,
            in int descStartIndex,
            in double fillModulus)
        {
            for (int i = 0; i < blockWidth; i++)
            {
                output[blockStartIndex + i] =
                    singleBlockProgress * i <= fillModulus ?
                    blockContent : '\0';
            }

            var desc = String.Format(
                DESC_FORMAT,
                fillModulus).ToCharArray();

            for (int i = 0; i < desc.Length - 1; i++)
            {
                output[descStartIndex + i] = desc[i];
            }

            return output;
        }

        /// <summary>
        /// ���½���
        /// </summary>
        /// <param name="value">��ǰ����ֵ</param>
        public void UpdateProgress(double value)
        {
            lock (locker)
            {
                var currentCursorPosition =
                   (left: Console.CursorLeft, top: Console.CursorTop);
                var fillModulus = CalculateFillModulus(
                    value,
                    this.minValue,
                    this.maxValue);
                this.output = UpdateOutput(
                    this.output,
                    this.blockWidth,
                    this.blockStartIndex,
                    this.blockContent,
                    this.blockSingleModulus,
                    this.descStartIndex,
                    fillModulus);

                EraseOutput(this.barLeft, this.barTop, this.output.Length);

                // reset cursor to bar start position
                Console.SetCursorPosition(this.barLeft, this.barTop);

                DisplayOutput(this.output);

                // reset cursor to original position
                Console.SetCursorPosition(
                    currentCursorPosition.left,
                    currentCursorPosition.top);
            }
        }

        /// <summary>
        /// �������ϵ��
        /// </summary>
        /// <param name="value">��ǰ����ֵ</param>
        /// <param name="minValue">��С����ֵ</param>
        /// <param name="maxValue">������ֵ</param>
        /// <returns>���ϵ��</returns>
        private double CalculateFillModulus(
            in double value,
            in double minValue,
            in double maxValue)
        {
            return value >= minValue ?
                ((value - minValue) / (maxValue - minValue)) * 100 : 0;
        }

        /// <summary>
        /// ��ʾ�������
        /// </summary>
        private void DisplayOutput(char[] output)
        {
            Console.Write(this.output);
        }

        /// <summary>
        /// Ĩȥ�������
        /// </summary>
        /// <param name="cursorLeft">�������λ��</param>
        /// <param name="cursorTop">�������λ��</param>
        /// <param name="width">Ҫ�����Ŀ��</param>
        public void EraseOutput(
            in int cursorLeft,
            in int cursorTop,
            in int width)
        {
            Console.SetCursorPosition(cursorLeft, cursorTop);

            for (int i = 0; i < width; i++)
            {
                Console.Write("\0");
            }
        }

        /// <summary>
        /// ��ʾ������
        /// </summary>
        /// <returns>��ǰʵ��</returns>
        public ConsoleProgressBar Show()
        {
            lock (locker)
            {
                DisplayOutput(this.output);
                return this;
            }
        }
    }
}