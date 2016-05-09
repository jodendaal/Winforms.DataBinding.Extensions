# Winforms.DataBinding.Extensions

Strongly typed databindings for win forms. Ideal for MVVM applications.

Example 
--------------------------------

	txtDescription.Bind<TextBox, IConnectionDetailsViewModel>(t => t.Text, _viewModel, vm => vm.Description, DataSourceUpdateMode.OnValidation);


	connectionsGridView1.Bind<ConnectionsGridView, IConnectionsViewModel>(t => t.DataSource, _viewModel, vm => vm.Connections, DataSourceUpdateMode.OnPropertyChanged);