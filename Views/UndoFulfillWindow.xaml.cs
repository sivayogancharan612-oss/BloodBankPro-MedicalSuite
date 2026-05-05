using System.Windows;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using BloodBankPro.Database;
using BloodBankPro.Models;

namespace BloodBankPro.Views
{
    public partial class UndoFulfillWindow : Window
    {
        private readonly SqlConnection _con;
        private readonly SqlTransaction _tx;
        private readonly BloodRequest   _req;
        private readonly DispatcherTimer _timer = new();
        private int _seconds = 10;
        private bool _decided = false;

        public bool WasUndone { get; private set; } = false;

        public UndoFulfillWindow(SqlConnection con, SqlTransaction tx, BloodRequest req)
        {
            InitializeComponent();
            _con = con; _tx = tx; _req = req;

            TxtDetail.Text = $"{req.UnitsNeeded} unit(s) of {req.BloodType} allocated for {req.PatientName}\n({req.HospitalName})";

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            _seconds--;
            TxtCountdown.Text = _seconds.ToString();

            if (_seconds <= 0)
            {
                _timer.Stop();
                Commit();
            }
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _decided = true;
            WasUndone = true;
            DatabaseHelper.RollbackFulfill(_con, _tx);
            Close();
        }

        private void Commit()
        {
            if (_decided) return;
            _decided = true;
            DatabaseHelper.CommitFulfill(_con, _tx, _req);
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            base.OnClosed(e);
        }
    }
}
