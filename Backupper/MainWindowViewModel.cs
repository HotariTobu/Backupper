namespace Backupper
{
    class MainWindowViewModel : SharedWPF.ViewModelBase
    {
        #region == FromPathText ==

        private string _FromPathText;
        public string FromPathText
        {
            get => _FromPathText;
            set
            {
                if (value != null && !value.Equals(_FromPathText))
                {
                    _FromPathText = value;
                    RaisePropertyChanged(nameof(FromPathText));
                }
            }
        }

        #endregion

        #region == ToPathText ==

        private string _ToPathText;
        public string ToPathText
        {
            get => _ToPathText;
            set
            {
                if (value != null && !value.Equals(_ToPathText))
                {
                    _ToPathText = value;
                    RaisePropertyChanged(nameof(ToPathText));
                }
            }
        }

        #endregion

        #region == TreeSource1 ==

        private TreeNodeCollection _TreeSource1;
        public TreeNodeCollection TreeSource1
        {
            get => _TreeSource1;
            set
            {
                if (value != null && _TreeSource1 != value)
                {
                    _TreeSource1 = value;
                    RaisePropertyChanged(nameof(TreeSource1));
                }
            }
        }

        #endregion

        #region == TreeSource2 ==

        private TreeNodeCollection _TreeSource2;
        public TreeNodeCollection TreeSource2
        {
            get => _TreeSource2;
            set
            {
                if (value != null && _TreeSource2 != value)
                {
                    _TreeSource2 = value;
                    RaisePropertyChanged(nameof(TreeSource2));
                }
            }
        }

        #endregion

        #region == CanCheck ==

        private bool _CanCheck;
        public bool CanCheck
        {
            get => _CanCheck;
            set
            {
                if (_CanCheck != value)
                {
                    _CanCheck = value;
                    RaisePropertyChanged(nameof(CanCheck));
                }
            }
        }

        #endregion

        #region == CanBackup ==

        private bool _CanBackup;
        public bool CanBackup
        {
            get => _CanBackup;
            set
            {
                if (_CanBackup != value)
                {
                    _CanBackup = value;
                    RaisePropertyChanged(nameof(CanBackup));
                }
            }
        }

        #endregion

        #region == CanCancel ==

        private bool _CanCancel;
        public bool CanCancel
        {
            get => _CanCancel;
            set
            {
                if (_CanCancel != value)
                {
                    _CanCancel = value;
                    RaisePropertyChanged(nameof(CanCancel));
                }
            }
        }

        #endregion

        #region == IsIndeterminate ==

        private bool _IsIndeterminate;
        public bool IsIndeterminate
        {
            get => _IsIndeterminate;
            set
            {
                if (_IsIndeterminate != value)
                {
                    _IsIndeterminate = value;
                    RaisePropertyChanged(nameof(IsIndeterminate));
                }
            }
        }

        #endregion

        #region == Progress ==

        private int _Progress;
        public int Progress
        {
            get => _Progress;
            set
            {
                if (_Progress != value)
                {
                    _Progress = value;
                    if (_Progress >= 0 && _Progress <= 100)
                    {
                        ClearErrror(nameof(Progress));
                    }
                    else
                    {
                        SetError(nameof(Progress), "Error: argument out of range");
                    }
                    RaisePropertyChanged(nameof(Progress));
                }
            }
        }

        #endregion

        #region == MaxProgress ==

        private int _MaxProgress;
        public int MaxProgress
        {
            get => _MaxProgress;
            set
            {
                if (_MaxProgress != value)
                {
                    _MaxProgress = value;
                    if (_MaxProgress >= 0 && _MaxProgress <= 10)
                    {
                        ClearErrror(nameof(MaxProgress));
                    }
                    else
                    {
                        SetError(nameof(MaxProgress), "Error: argument out of range");
                    }
                    RaisePropertyChanged(nameof(MaxProgress));
                }
            }
        }

        #endregion

        #region == None ==

        private int _None;
        public int None
        {
            get => _None;
            set
            {
                if (_None != value)
                {
                    _None = value;
                    if (_None >= 0 && _None <= 10)
                    RaisePropertyChanged(nameof(None));
                }
            }
        }

        #endregion

        #region == Newer ==

        private int _Newer;
        public int Newer
        {
            get => _Newer;
            set
            {
                if (_Newer != value)
                {
                    _Newer = value;
                    RaisePropertyChanged(nameof(Newer));
                }
            }
        }

        #endregion

        #region == Older ==

        private int _Older;
        public int Older
        {
            get => _Older;
            set
            {
                if (_Older != value)
                {
                    _Older = value;
                    RaisePropertyChanged(nameof(Older));
                }
            }
        }

        #endregion

        #region == Missing ==

        private int _Missing;
        public int Missing
        {
            get => _Missing;
            set
            {
                if (_Missing != value)
                {
                    _Missing = value;
                    RaisePropertyChanged(nameof(Missing));
                }
            }
        }

        #endregion

        public MainWindowViewModel()
        {
            FromPathText = string.Empty;
            ToPathText = string.Empty;
            TreeSource1 = new TreeNodeCollection();
            TreeSource2 = new TreeNodeCollection();
            CanCheck = true;
            CanBackup = true;
            CanCancel = true;
            ResetProperties1();
            ResetProperties2();
        }

        public void ResetProperties1()
        {
            CanCheck = true;
            IsIndeterminate = false;
            Progress = 0;
            MaxProgress = 1;
        }

        public void ResetProperties2()
        {
            None = 0;
            Newer = 0;
            Older = 0;
            Missing = 0;
        }
    }
}