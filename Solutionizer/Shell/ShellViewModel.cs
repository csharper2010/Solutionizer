﻿using System.Windows;
using Caliburn.Micro;
using Ookii.Dialogs.Wpf;
using Solutionizer.FileScanning;
using Solutionizer.Infrastructure;
using Solutionizer.ProjectRepository;
using Solutionizer.Settings;
using Solutionizer.Solution;
using Solutionizer.Update;
using Solutionizer.ViewModels;

namespace Solutionizer.Shell {
    using System.ComponentModel.Composition;

    [Export(typeof(IShell))]
    public sealed class ShellViewModel : Screen, IShell {
        private readonly Services.Settings _settings;
        private readonly IDialogManager _dialogManager;
        private readonly IUpdateManager _updateManager;
        private readonly ProjectRepositoryViewModel _projectRepository;
        private SolutionViewModel _solution;

        [ImportingConstructor]
        public ShellViewModel(Services.Settings settings, IDialogManager dialogManager, IUpdateManager updateManager) {
            _settings = settings;
            _projectRepository = new ProjectRepositoryViewModel(settings);
            _dialogManager = dialogManager;
            _updateManager = updateManager;
            DisplayName = "Solutionizer";
        }

        public ProjectRepositoryViewModel ProjectRepository {
            get { return _projectRepository; }
        }

        public SolutionViewModel Solution {
            get { return _solution; }
            set {
                if (_solution != value) {
                    _solution = value;
                    NotifyOfPropertyChange(() => Solution);
                }
            }
        }

        public Services.Settings Settings {
            get { return _settings; }
        }

        protected override void OnViewLoaded(object view) {
            base.OnViewLoaded(view);

            _updateManager.CheckForUpdate().ContinueWith(t => {
                if (t.IsCompleted) {
                    var updateInfo = t.Result;
                    if (updateInfo != null) {
                        _updateManager.DownloadPackage(updateInfo);
                    }
                }
            });

            if (_settings.ScanOnStartup) {
                LoadProjects(_settings.RootPath);
            }
        }

        public void SelectRootPath() {
            var dlg = new VistaFolderBrowserDialog {
                SelectedPath = _settings.RootPath
            };
            if (dlg.ShowDialog(Application.Current.MainWindow) == true) {
                _settings.RootPath = dlg.SelectedPath;
                LoadProjects(dlg.SelectedPath);
            }
        }

        public void ShowSettings() {
            _dialogManager.ShowDialog(new SettingsViewModel(_settings));
        }

        public IDialogManager Dialogs {
            get { return _dialogManager; }
        }

        private void LoadProjects(string path) {
            var fileScanningViewModel = new FileScanningViewModel(_settings, path);
            _dialogManager.ShowDialog(fileScanningViewModel);

            fileScanningViewModel.Deactivated += (sender, args) => {
                if (fileScanningViewModel.Result != null) {
                    _projectRepository.RootPath = path;
                    _projectRepository.RootFolder = fileScanningViewModel.Result.ProjectFolder;
                    Solution = new SolutionViewModel(_settings, path, fileScanningViewModel.Result.Projects);
                    DisplayName = "Solutionizer - " + path;
                }
            };
        }

        public void OnDoubleClick(ItemViewModel itemViewModel) {
            var projectViewModel = itemViewModel as ProjectViewModel;
            if (projectViewModel != null) {
                _solution.AddProject(projectViewModel.Project);
            }
        }
    }
}