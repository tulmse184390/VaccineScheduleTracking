﻿namespace VaccineScheduleTracking.API_Test.Configurations
{
    public class DefaultConfig
    {
        public int SlotDuration { get; set; } // Độ dài mỗi slot (phút)
        public int ScheduleLength { get; set; } // Độ dài lịch hẹn (ngày)
        public int OverdueSchedule { get; set; } // xóa lịch hoàn toàn (ngày)
        public int PeriodForVaccine { get; set; } // Chu kỳ tiêm vaccine (ngày)
        public int MailDueDate { get; set; } // Ngày gửi mail nhắc nhở ( > ngày hiện tại ?? ngày)

    }

    public class AdminAccountConfig
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
