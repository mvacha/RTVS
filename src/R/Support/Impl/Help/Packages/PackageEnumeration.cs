﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Packages
{
    /// <summary>
    /// Implements enumerator of packages that is based
    /// on the particular collection install path.
    /// Package names normally match names of folders
    /// the packages are installed in.
    /// </summary>
    internal class PackageEnumeration : IEnumerable<IPackageInfo>
    {
        private string _libraryPath;

        public PackageEnumeration(string libraryPath)
        {
            _libraryPath = libraryPath;
        }

        public IEnumerator<IPackageInfo> GetEnumerator()
        {
            return new PackageEnumerator(_libraryPath);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    class PackageEnumerator : IEnumerator<IPackageInfo>
    {
        private IEnumerator<string> _directoriesEnumerator;

        public PackageEnumerator(string libraryPath)
        {
            _directoriesEnumerator = Directory.EnumerateDirectories(libraryPath).GetEnumerator();
        }

        public IPackageInfo Current
        {
            get
            {
                string directoryPath = _directoriesEnumerator.Current;
                string name = Path.GetFileName(directoryPath).ToLowerInvariant();

                return new PackageInfo(name, Path.GetDirectoryName(directoryPath));
            }
        }

        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        public bool MoveNext()
        {
            return _directoriesEnumerator.MoveNext();
        }

        public void Reset()
        {
            _directoriesEnumerator.Reset();
        }

        public void Dispose()
        {
            if (_directoriesEnumerator != null)
            {
                _directoriesEnumerator.Dispose();
                _directoriesEnumerator = null;
            }
        }
    }
}
