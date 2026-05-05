using BloodBankPro.Database;
using BloodBankPro.Models;

namespace BloodBankPro.Repositories;

public class DonorRepository
{
    public List<Donor> GetAll() => DatabaseHelper.GetDonors();

    public void Add(Donor donor) => DatabaseHelper.AddDonor(donor);

    public void Update(Donor donor) => DatabaseHelper.UpdateDonor(donor);

    public void Delete(int donorId, string fullName) => DatabaseHelper.DeleteDonor(donorId, fullName);
}
