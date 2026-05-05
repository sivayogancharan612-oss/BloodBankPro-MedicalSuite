using BloodBankPro.Models;
using BloodBankPro.Repositories;
using System;
using System.Windows.Input;
using BloodBankPro.Database;

namespace BloodBankPro.ViewModels;

public class DonorViewModel
{
    private readonly DonorRepository _repository;
    private List<Donor> _all = new();

    public DonorViewModel(DonorRepository repository)
    {
        _repository = repository;
    }

    public int TotalCount => _all.Count;

    public List<Donor> Load()
    {
        _all = _repository.GetAll();
        return _all;
    }

    public List<Donor> Filter(string query, string bloodType, string status)
    {
        var q = (query ?? string.Empty).Trim().ToLower();
        var result = _all.AsEnumerable();

        if (!string.IsNullOrEmpty(q))
        {
            result = result.Where(d =>
                d.FullName.ToLower().Contains(q) ||
                d.Phone.ToLower().Contains(q) ||
                d.Email.ToLower().Contains(q));
        }

        if (!string.Equals(bloodType, "All Types", StringComparison.Ordinal))
        {
            result = result.Where(d => d.BloodType == bloodType);
        }

        if (!string.Equals(status, "All Status", StringComparison.Ordinal))
        {
            result = result.Where(d => d.Status == status);
        }

        return result.ToList();
    }

    public void Save(Donor donor)
    {
        if (donor.Id > 0)
        {
            _repository.Update(donor);
        }
        else
        {
            _repository.Add(donor);
        }

        AppEvents.RaiseDonorsChanged();
    }

    public void Delete(Donor donor)
    {
        _repository.Delete(donor.Id, donor.FullName);
        AppEvents.RaiseDonorsChanged();
    }

    public ICommand RevealPhoneCommand => new RevealCommand();

    private class RevealCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is Donor donor && !donor.IsPhoneRevealed)
            {
                donor.IsPhoneRevealed = true;
                DatabaseHelper.Log("REVEAL", "Donors", $"Staff revealed phone number for donor ID {donor.Id}");
            }
        }
    }
}
