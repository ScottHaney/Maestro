using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ComponentsVSExtension
{
    /// <summary>
    /// Events related to the editor documents.
    /// </summary>
    public class DocumentEventsCopy : IVsRunningDocTableEvents, IVsRunningDocTableEvents4
    {
        private readonly RunningDocumentTable _rdt;

        internal DocumentEventsCopy()
        {
            _rdt = new RunningDocumentTable();
            _rdt.Advise(this);
        }

        /// <summary>
        /// Happens when a file is saved to disk.
        /// </summary>
        public event Action<string>? Saved;

        /// <summary>
        /// Fires after the document was opened in the editor.
        /// </summary>
        /// <remarks>
        /// The event is called for documents in the document well but also
        /// for project files and may also be called for solution files.<br/>
        /// The document name may also be a special (generated name) in the form of
        /// RDT_PROJ_MK::{A2FE74E1-B743-11d0-AE1A-00A0C90FFFC3} that is used by 
        /// Visual Studio for the 'miscellaneous files' project 
        /// </remarks>
        public event Action<string>? Opened;

        /// <summary>
        /// Fires after the document was closed.
        /// </summary>
        /// <remarks>
        /// The event is called for documents in the document well but also
        /// for project files and may also be called for solution files. <br/>
        /// The document name may also be a special (generated name) in the form of
        /// RDT_PROJ_MK::{A2FE74E1-B743-11d0-AE1A-00A0C90FFFC3} that is used by 
        /// Visual Studio for the 'miscellaneous files' project 
        /// </remarks>
        public event Action<string>? Closed;

        /// <summary>
        /// Fires before the document takes focus.
        /// </summary>
        public event Action<DocumentView>? BeforeDocumentWindowShow;

        /// <summary>
        /// Fires after the document lost focus.
        /// </summary>
        public event Action<DocumentView>? AfterDocumentWindowHide;

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            // Please note that this event is called multiple times when a document
            // is opened for editing.
            // This code tries to only call the Open Event once
            // 
            if (dwEditLocksRemaining == 1 && dwReadLocksRemaining == 0)
            {
                if (Opened != null)
                {
                    string file = _rdt.GetDocumentInfo(docCookie).Moniker;
                    Opened.Invoke(file);
                }
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            // Please note that this event is called multiple times when a document
            // is opened for editing.
            // This code tries to only call the Close Event once
            if (dwReadLocksRemaining == 0 && dwEditLocksRemaining == 0)
            {
                if (Closed != null)
                {
                    string file = _rdt.GetDocumentInfo(docCookie).Moniker;

                    if (!string.IsNullOrEmpty(file))
                    {
                        Closed.Invoke(file);
                    }
                }
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            if (Saved != null)
            {
                string file = _rdt.GetDocumentInfo(docCookie).Moniker;
                Saved?.Invoke(file);
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            if (BeforeDocumentWindowShow != null && fFirstShow == 1)
            {
                var docView = CreateInstance<DocumentView>(new[] { pFrame });
                BeforeDocumentWindowShow.Invoke(docView);
            }

            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            if (AfterDocumentWindowHide != null)
            {
                var docView = CreateInstance<DocumentView>(new[] { pFrame });
                AfterDocumentWindowHide.Invoke(docView);
            }

            return VSConstants.S_OK;
        }

        public int OnBeforeFirstDocumentLock(IVsHierarchy pHier, uint itemid, string pszMkDocument)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();

            /*if (LinkFile.TryParse(pszMkDocument, out var linkFile))
            {
                if (_rdt.FindDocument(linkFile.LinkedFilePath) == null)
                    VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, linkFile.LinkedFilePath, Guid.Empty, out _, out _, out IVsWindowFrame? frame);
            }*/

            return VSConstants.S_OK;
        }

        public int OnAfterSaveAll()
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLastDocumentUnlock(IVsHierarchy pHier, uint itemid, string pszMkDocument, int fClosedWithoutSaving)
        {
            return VSConstants.S_OK;
        }

        private static T CreateInstance<T>(params object[] args)
        {
            var type = typeof(T);
            var instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);
            return (T)instance;
        }
    }

    public class LinkFile
    {
        public readonly string FilePath;
        public string LinkedFilePath => Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileNameWithoutExtension(FilePath));

        private LinkFile(string filePath)
        {
            FilePath = filePath;
        }

        public static bool TryParse(string filePath, out LinkFile linkFile)
        {
            if (string.Compare(Path.GetExtension(filePath), ".link", StringComparison.OrdinalIgnoreCase) == 0)
            {
                linkFile = new LinkFile(filePath);
                return true;
            }
            else
            {
                linkFile = null;
                return false;
            }

        }
    }
}
