# Winforms.DataBinding.Extensions

Strongly typed databindings for winforms. Ideal for MVVM applications.

Nuget 
--------------------------------
Install-Package Winforms.DataBindings.Extensions

https://www.nuget.org/packages/Winforms.DataBindings.Extensions/

Databinding Example
--------------------------------

	txtDescription.Bind<TextBox, IConnectionDetailsViewModel>(t => t.Text, _viewModel, vm => vm.Description, DataSourceUpdateMode.OnValidation);


	connectionsGridView1.Bind<ConnectionsGridView, IConnectionsViewModel>(t => t.DataSource, _viewModel, vm => vm.Connections, DataSourceUpdateMode.OnPropertyChanged);
	

Error Provider Example
--------------------------------
The error provider extensions allow you to use the errorProvider with System.ComponentModel.DataAnnotations.
	
	public class ViewModel
	{
		[Required(ErrorMessage = "Description is Required")]
		[StringLength(maximumLength:5,MinimumLength = 2)]
		public string Description { get; set; }
	}
	
		
	txtDescription.Bind<TextBox, ViewModel>(t => t.Text, _viewModel, vm => vm.Description, DataSourceUpdateMode.OnValidation);
	errorProvider1.RegisterFormBindings(this);
