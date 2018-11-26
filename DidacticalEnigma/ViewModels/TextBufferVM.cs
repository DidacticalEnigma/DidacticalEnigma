using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using DidacticalEnigma.Core.Models.LanguageService;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{
    public class TextBufferVM : INotifyPropertyChanged
    {
        public ObservableBatchCollection<LineVM> Lines { get; } = new ObservableBatchCollection<LineVM>();

        private string rawOutput = "";

        private readonly IMorphologicalAnalyzer<IEntry> morphologicalAnalyzer;
        private readonly IKanjiProperties kanji;
        private readonly IKanaProperties kana;
        private readonly IRelated related;

        public string RawOutput
        {
            get => rawOutput;
            set
            {
                if (rawOutput == value)
                    return;
                rawOutput = value;
                SetAnnotations(rawOutput);
                OnPropertyChanged();
            }
        }

        private int selectionLength;
        public int SelectionLength
        {
            get => selectionLength;
            set
            {
                if (selectionLength == value)
                    return;
                selectionLength = value;
                OnPropertyChanged();
            }
        }

        private int caretIndex;
        public int CaretIndex
        {
            get => caretIndex;
            set
            {
                if (caretIndex == value)
                    return;
                caretIndex = value;
                OnPropertyChanged();
            }
        }

        private string selectedText;
        public string SelectedText
        {
            get => selectedText;
            set
            {
                if (selectedText == value)
                    return;
                selectedText = value;
                OnPropertyChanged();
            }
        }

        private SelectionInfoVM selectionInfo;
        public SelectionInfoVM SelectionInfo
        {
            get => selectionInfo;

            set
            {
                if (selectionInfo == value)
                    return;
                selectionInfo = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand IssueMeCabSplit { get; }

        public RelayCommand InsertTextAtCaret { get; }

        public string Name { get; }

        private void AddIteration()
        {
            for (int i = 0; i < Lines.Count; ++i)
            {
                for (int j = 0; j < Lines[i].Words.Count; ++j)
                {
                    int startI = i;
                    int startJ = j;
                    Lines[i].Words[j].SubsequentWords = () => Iterate(startI, startJ);
                }
            }

            IEnumerable<WordVM> Iterate(int startI, int startJ)
            {
                for(int j = startJ; j < Lines[startI].Words.Count; ++j)
                {
                    yield return Lines[startI].Words[j];
                }
                for(int i = startI+1; i < Lines.Count; ++i)
                {
                    for(int j = 0; j < Lines[i].Words.Count; ++j)
                    {
                        yield return Lines[i].Words[j];
                    }
                }
            }
        }

        private void SetAnnotations(string unannotatedOutput)
        {
            Lines.Clear();
            Lines.AddRange(
                unannotatedOutput
                    .Split(new []{"\r\n", "\n", "\r"}, StringSplitOptions.None)
                    .Select(rawSentence => rawSentence.Split(new[]{" ", "　"}, StringSplitOptions.None))
                    .Select(sentence => new LineVM(sentence.Select(word => new WordVM(new WordInfo(word), kanji, kana, related)))));
            AddIteration();
            rawOutput = string.Join(
                "\n",
                Lines.Select(
                    line => string.Join(
                        " ",
                        line.Words.Select(word => word.StringForm))));
        }

        private void SetAnnotationsMeCab(string unannotatedOutput)
        {
            Lines.Clear();
            Lines.AddRange(
                morphologicalAnalyzer.BreakIntoSentences(unannotatedOutput)
                    .Select(sentence => new LineVM(sentence.Select(word => new WordVM(word, kanji, kana, related)))));
            AddIteration();
            rawOutput = string.Join(
                "\n",
                Lines.Select(
                    line => string.Join(
                        " ",
                        line.Words.Select(word => word.StringForm))));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TextBufferVM(
            string name,
            IMorphologicalAnalyzer<IEntry> morphologicalAnalyzer,
            IKanjiProperties kanji,
            IKanaProperties kana,
            IRelated related)
        {
            this.morphologicalAnalyzer = morphologicalAnalyzer;
            this.kanji = kanji;
            this.kana = kana;
            this.related = related;
            Name = name;
            InsertTextAtCaret = new RelayCommand((s) =>
            {
                var inputStr = s.ToString();
                // TODO: insert at caret, not at the end of the string
                RawOutput += inputStr;
            });
            IssueMeCabSplit = new RelayCommand(() =>
            {
                SetAnnotationsMeCab(RawOutput);
                OnPropertyChanged(nameof(RawOutput));
            });
        }
    }
}