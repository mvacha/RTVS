﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Windows {
    internal abstract class ToolWindowPaneFactory<T> where T : RToolWindowPane, IVisualComponentContainer<IVisualComponent> {
        private readonly Dictionary<int, ToolWindowPaneHolder> _toolWindowPanes = new Dictionary<int, ToolWindowPaneHolder>();

        protected T GetOrCreate(int instanceId, Func<int, T> factory) {
            ToolWindowPaneHolder holder;
            if (_toolWindowPanes.TryGetValue(instanceId, out holder)) {
                if (holder.ToolWindowPane != null) {
                    return holder.ToolWindowPane;
                }
                RemoveHolder(instanceId);
            }

            var instance = factory(instanceId);
            IVsUIShell vsUiShell = VsAppShell.Current.GlobalServices.GetService<IVsUIShell>(typeof(SVsUIShell));
            ToolWindowUtilities.CreateToolWindow(vsUiShell, instance, instanceId);

            holder = new ToolWindowPaneHolder(instance, () => RemoveHolder(instanceId));
            _toolWindowPanes.Add(instanceId, holder);
            return instance;
        }

        private void RemoveHolder(int id) {
            ToolWindowPaneHolder holder;
            if (!_toolWindowPanes.TryGetValue(id, out holder)) {
                return;
            }
            _toolWindowPanes.Remove(id);
            holder.Dispose();
        }

        private class ToolWindowPaneHolder : IVsWindowFrameNotify, IVsWindowFrameNotify3, IDisposable {
            private readonly Action _onClose;
            private readonly IDisposable _subscription;
            public T ToolWindowPane { get; private set; }

            public ToolWindowPaneHolder(T toolWindowPane, Action onClose) {
                _onClose = onClose;
                ToolWindowPane = toolWindowPane;

                var frame = (IVsWindowFrame2)ToolWindowPane.Frame;
                uint cookie;
                ErrorHandler.ThrowOnFailure(frame.Advise(this, out cookie));
                _subscription = Disposable.Create(() => frame.Unadvise(cookie));
            }

            private void OnShow(int fShow) {
                if (ToolWindowPane == null) {
                    return;
                }

                switch (fShow) {
                    case (int)__FRAMESHOW.FRAMESHOW_WinClosed:
                    case (int)__FRAMESHOW.FRAMESHOW_DestroyMultInst:
                        _subscription.Dispose();
                        _onClose();
                        break;
                }
            }

            public void Dispose() {
                _subscription.Dispose();
                ToolWindowPane = null;
            }

            int IVsWindowFrameNotify.OnShow(int fShow) {
                OnShow(fShow);
                return VSConstants.S_OK;
            }

            int IVsWindowFrameNotify.OnSize() {
                return VSConstants.S_OK;
            }

            int IVsWindowFrameNotify.OnDockableChange(int fDockable) {
                return VSConstants.S_OK;
            }

            int IVsWindowFrameNotify3.OnMove(int x, int y, int w, int h) {
                return VSConstants.S_OK;
            }

            int IVsWindowFrameNotify3.OnSize(int x, int y, int w, int h) {
                return VSConstants.S_OK;
            }

            int IVsWindowFrameNotify3.OnDockableChange(int fDockable, int x, int y, int w, int h) {
                return VSConstants.S_OK;
            }

            int IVsWindowFrameNotify3.OnClose(ref uint pgrfSaveOptions) {
                return VSConstants.S_OK;
            }

            int IVsWindowFrameNotify.OnMove() {
                return VSConstants.S_OK;
            }

            int IVsWindowFrameNotify3.OnShow(int fShow) {
                OnShow(fShow);
                return VSConstants.S_OK;
            }
        }
    }
}