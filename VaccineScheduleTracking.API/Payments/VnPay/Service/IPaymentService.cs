﻿using VaccineScheduleTracking.API_Test.Payments.VnPay.Models;

namespace VaccineScheduleTracking.API_Test.Payments.VnPay.Service
{
    public interface IPaymentService
    {
        Task<Models.Payment> AddPaymentAsync(Models.Payment model);
        Task<VnPayTransaction> AddPaymentVnPayTransactionAsyn(VnPayTransaction vnPayTransaction);
        Task<List<Models.Payment>> GetPaymentsByAccountId(int id);
    }
}
