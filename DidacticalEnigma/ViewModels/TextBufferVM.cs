using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DidacticalEnigma.Models;
using DidacticalEnigma.ViewModels;

namespace DidacticalEnigma
{
    public class TextBufferVM : INotifyPropertyChanged
    {
        public ObservableBatchCollection<LineVM> Lines { get; } = new ObservableBatchCollection<LineVM>();

        private string rawOutput = "";

        private readonly ILanguageService lang;

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

        private void SetAnnotations(string unannotatedOutput)
        {
            Lines.Clear();
            Lines.AddRange(
                unannotatedOutput
                    .Split(new []{"\r\n", "\n", "\r"}, StringSplitOptions.None)
                    .Select(rawSentence => rawSentence.Split(' '))
                    .Select(sentence => new LineVM(sentence.Select(word => new WordVM(new WordInfo(word, ""), lang)))));
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
                lang.BreakIntoSentences(unannotatedOutput)
                    .Select(sentence => new LineVM(sentence.Select(word => new WordVM(word, lang)))));
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

        public TextBufferVM(string name, ILanguageService lang)
        {
            this.lang = lang;
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