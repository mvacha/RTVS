// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.ConnectionManager.Commands {
    public class ReconnectCommand : IAsyncCommand {
        private readonly IConnectionManager _connectionManager;
        private readonly ICoreShell _shell;

        public ReconnectCommand(IRInteractiveWorkflow workflow) {
            _connectionManager = workflow.Connections;
            _shell = workflow.Shell;
        }

        public CommandStatus Status => _connectionManager.IsConnected 
            ? CommandStatus.Supported
            : CommandStatus.SupportedAndEnabled;

        public Task InvokeAsync() {
            var connection = _connectionManager.ActiveConnection;
            if (connection != null && !_connectionManager.IsConnected) {
                _shell.ProgressDialog.Show(_connectionManager.ReconnectAsync, Resources.ConnectionManager_ReconnectionToProgressBarMessage.FormatInvariant(connection.Name));
            }
            return Task.CompletedTask;
        }
    }
}