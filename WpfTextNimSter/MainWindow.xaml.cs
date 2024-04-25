﻿using System.DirectoryServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTextNimSter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string RemoveLines(string src)
        {
            return src.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ");
        }

        private string RemoveSpaces(string src)
        {
            return src.Replace(" ", "");
        }

        private string RemoveHyphen(string src)
        {
            return src.Replace("- ", "");
        }

        /// <summary>
        /// similar to C strspn or C++ std::string::find_first_not_of
        /// </summary>
        private int FindFirstNotOf(string src, string key)
        {
            for(int i = 0; i < src.Length; ++i)
            {
                bool matched = false;
                for(int ii = 0; ii < key.Length; ++ii)
                {
                    if (src[i] == key[ii])
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched) return i;
            }
            return -1;
        }

        /// <summary>
        /// Remove tab and the following line number. 
        /// </summary>
        /// <example>
        /// per uiginti\t1.pr.1.1\nannos
        /// => per uiginti annos
        /// </example>
        private string RemoveTabAndLineNumber(string src)
        {
            // There should be better C# way
            string removed = "";
            for(int read = 0, length = 0; read < src.Length;)
            {
                int pos = src[read..].IndexOf('\t');
                if (pos == -1)
                {
                    length = src.Length - read;
                    removed += src.Substring(read, length);
                    break;
                }

                length = pos;
                removed += src.Substring(read, length);
                read += pos;
                removed += " ";

                pos = FindFirstNotOf(src[read..], "0123456789. \t\r\n");
                if(pos != -1)
                {
                    int pos2 = src[read..].IndexOf("pr.");
                    if(pos != -1 && pos2 == pos)
                    {
                        read += pos2 + 3; // length of "pr."
                        pos = FindFirstNotOf(src[read..], "0123456789. \t\r\n");
                    }
                }
                if (pos == -1)
                {
                    length = src.Length - read;
                    removed += src.Substring(read, length);
                    break;
                }
                read += pos;
            }
            return removed;
        }

        private enum RemovalAction
        {
            Lines,
            Spaces,
            Tabs,
            TabsAndFollowingString,
        }

        private RemovalAction GuessLikelyAction(string src)
        {
            // prefer foreach to Regex when the pattern is single char
            Func<string, char, int> CountCharOccurence = (src, key)
                =>
            {
                int count = 0;
                foreach (char c in src)
                {
                    if (c == key) ++count;
                }
                return count;
            };
            int LineCount = CountCharOccurence(src, '\r') + CountCharOccurence(src, '\n');
            int SpaceCount = CountCharOccurence(src, ' ');
            int TabCount = CountCharOccurence(src, '\t');

            float SpaceRatio = (float)src.Length / SpaceCount;

            RemovalAction action;
            if (SpaceRatio < 3.0f) action = RemovalAction.Spaces;
            else if (TabCount > LineCount / 5)action = RemovalAction.TabsAndFollowingString;
            else action = RemovalAction.Lines;
            return action;
        }

        private void textBoxRaw_TextChanged(object sender, TextChangedEventArgs e)
        {
            string allText = textBoxRaw.Text;
            RemovalAction removalAction = GuessLikelyAction(allText);

            string formatted;
            switch(removalAction)
            {
                case RemovalAction.Spaces:
                    formatted = RemoveLines(RemoveSpaces(allText));
                    break;
                case RemovalAction.TabsAndFollowingString:
                    formatted = RemoveLines(RemoveHyphen(RemoveTabAndLineNumber(allText)));
                    break;
                default:
                    formatted = RemoveLines(allText);
                    break;
            }
            textBoxFormatted.Text = formatted;
        }

        private void textBoxRaw_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string clipBoardText = System.Windows.Clipboard.GetText();
            textBoxRaw.Text = clipBoardText;
        }

        private void textBoxFormatted_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            textBoxFormatted.SelectAll();
            System.Windows.Clipboard.SetText(textBoxFormatted.Text);
        }
    }
}