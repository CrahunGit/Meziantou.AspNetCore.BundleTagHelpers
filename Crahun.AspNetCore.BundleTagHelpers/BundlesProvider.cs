using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BundlerMinifier;

namespace Crahun.AspNetCore.BundleTagHelpers
{
    public class BundleProvider : IBundleProvider, IDisposable
    {
        private readonly object _lock = new object();
        private readonly string _configurationPath;
        private IList<Bundle> _bundles;
        private FileSystemWatcher _fileWatcher;

        public BundleProvider()
            : this("bundleconfig.json")
        {
        }

        public BundleProvider(string configurationPath)
        {
            if (configurationPath == null) throw new ArgumentNullException(nameof(configurationPath));

            var fullPath = Path.GetFullPath(configurationPath);
            var directory = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileName(fullPath);
            _configurationPath = fullPath;

            if (directory != null)
            {
                var watcher = new FileSystemWatcher(directory);
                watcher.EnableRaisingEvents = true;
                watcher.IncludeSubdirectories = false;
                watcher.Filter = fileName;
                watcher.Changed += (sender, args) => Reset();
                watcher.Created += (sender, args) => Reset();
                watcher.Deleted += (sender, args) => Reset();
                _fileWatcher = watcher;
            }
        }

        private void Reset()
        {
            _bundles = null;
        }

        private void LoadBundles()
        {
            if (_bundles == null)
            {
                lock (_lock)
                {
                    if (_bundles == null)
                    {
                        if (!BundleHandler.TryGetBundles(_configurationPath, out var bundles))
                            throw new Exception("Unable to load bundles.");

                        var result = new List<Bundle>();
                        foreach (var bundle in bundles)
                        {
                            var b = new Bundle();
                            b.Name = bundle.OutputFileName;
                            b.OutputFileUrl = bundle.GetAbsoluteOutputFile();
                            b.InputFileUrls = bundle.GetAbsoluteInputFiles().ToList();
                            result.Add(b);
                        }

                        _bundles = result;
                    }
                }
            }
        }

        public Bundle GetBundle(string name)
        {
            LoadBundles();

            var bundle = _bundles.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));
            if (bundle != null)
                return bundle;

            return null;
        }

        public void Dispose()
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }
        }
    }
}