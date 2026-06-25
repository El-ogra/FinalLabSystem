using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Models;

namespace FinalLabSystem.Views.Patients;

public partial class ProfileSelectionDialog : Window
{
    public TestProfile? SelectedProfile { get; private set; }

    public ProfileSelectionDialog()
    {
        InitializeComponent();
    }

    public void LoadProfiles(IEnumerable<TestProfile> profiles)
    {
        ProfilesListBox.ItemsSource = profiles.ToList();
    }

    private void SelectButton_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesListBox.SelectedItem is TestProfile profile)
        {
            SelectedProfile = profile;
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("يرجى اختيار بروفايل", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void ProfilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ProfilesListBox.SelectedItem is TestProfile profile)
        {
            SelectedProfile = profile;
            DialogResult = true;
        }
    }
}
