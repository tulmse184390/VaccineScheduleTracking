﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using VaccineScheduleTracking.API.Helpers;
using VaccineScheduleTracking.API.Models.Entities;
using VaccineScheduleTracking.API_Test.Models.DTOs.Accounts;
using VaccineScheduleTracking.API_Test.Models.Entities;
using VaccineScheduleTracking.API_Test.Repository.Accounts;

namespace VaccineScheduleTracking.API_Test.Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IMapper mapper;
        private readonly IPasswordHasher<Account> passwordHasher;
        private readonly IEmailService emailService;
        private readonly JwtHelper jwtHelper;
        private readonly IWebHostEnvironment env;

        public AccountService(IAccountRepository accountRepository, IMapper mapper, IPasswordHasher<Account> passwordHasher, IEmailService emailService, JwtHelper jwtHelper, IWebHostEnvironment env)
        {
            this.accountRepository = accountRepository;
            this.mapper = mapper;
            this.passwordHasher = passwordHasher;
            this.emailService = emailService;
            this.jwtHelper = jwtHelper;
            this.env = env;
        }
        public async Task<Account?> LoginAsync(string username, string password)
        {
            var account = await accountRepository.GetAccountByUsernameAsync(username);
            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại!");
            }
            if (passwordHasher.VerifyHashedPassword(account, account.Password, password) != PasswordVerificationResult.Success)
            {
                throw new Exception("Sai mật khẩu!");
            }
            return mapper.Map<Account>(account);
        }

        public async Task<Account?> RegisterAsync(RegisterAccountDto registerAccount)
        {
            if (await accountRepository.GetAccountByUsernameAsync(registerAccount.Username) != null)
            {
                throw new Exception($"Tên người dùng: {registerAccount.Username} đã tồn tại");
            }
            if (await accountRepository.GetAccountByEmailAsync(registerAccount.Email) != null)
            {
                throw new Exception($"Địa chỉ email {registerAccount.Email} đã tồn tại ");
            }
            if (await accountRepository.GetAccountByPhonenumberAsync(registerAccount.PhoneNumber) != null)
            {
                throw new Exception($"Số điện thoại {registerAccount.PhoneNumber} đã tồn tại");
            }
            var fileName = "";
            if (registerAccount.Avatar != null)
            {
                if (string.IsNullOrEmpty(env.WebRootPath))
                {
                    env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                var uploadsFolder = Path.Combine(env.WebRootPath, "Images");
                Directory.CreateDirectory(uploadsFolder);

                fileName = $"{Guid.NewGuid()}{Path.GetExtension(registerAccount.Avatar.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await registerAccount.Avatar.CopyToAsync(stream);
                }
            }

            var account = mapper.Map<Account>(registerAccount);
            account.Status = "EMAILNOTACTIVE";
            var hashPawword = passwordHasher.HashPassword(account, account.Password);
            account.Password = hashPawword;
            account.Parent = new Parent() { Account = account };
            account.Avatar = $"/Images/{fileName}";

            var newAccount = await accountRepository.AddAccountAsync(account);

            var token = jwtHelper.GenerateEmailToken(newAccount.AccountID.ToString(), newAccount.Username, newAccount.Email, newAccount.PhoneNumber);
            string verificationLink = $"https://localhost:7270/api/Account/verify-email?token={Uri.EscapeDataString(token)}";

            string emailBody =
                  $@"<div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                <h2 style='color: #007bff;'>Xin chào {account.Firstname},</h2>
                <p>Chúng tôi rất vui khi bạn đăng ký tài khoản tại hệ thống của chúng tôi.</p>
                <p>Vui lòng nhấp vào nút bên dưới để xác minh email của bạn:</p>
                <div style='text-align: center; margin: 20px 0;'>
                    <a href='{verificationLink}' style='background-color: #007bff; color: #fff; padding: 12px 24px; text-decoration: none; font-size: 16px; border-radius: 5px; display: inline-block;'>
                        Xác minh Email
                    </a>
                </div>
                <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                <p><strong>Lưu ý:</strong> Liên kết sẽ hết hạn trong <strong>24 giờ</strong>.</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                <p style='text-align: center; font-size: 14px; color: #777;'>Trân trọng,<br>Đội ngũ hỗ trợ</p>
            </div>";
            await emailService.SendEmailAsync(account.Email, "Xác minh email", emailBody);

            return newAccount;
        }


        public async Task<Account?> RegisterBlankAccountAsync(RegisterBlankAccountDto registerAccount)
        {
            {
                if (await accountRepository.GetAccountByUsernameAsync(registerAccount.Username) != null)
                {
                    throw new Exception($"Tên người dùng: {registerAccount.Username} đã tồn tại");
                }
                if (await accountRepository.GetAccountByEmailAsync(registerAccount.Email) != null)
                {
                    throw new Exception($"Địa chỉ email {registerAccount.Email} đã tồn tại ");
                }
                if (await accountRepository.GetAccountByPhonenumberAsync(registerAccount.PhoneNumber) != null)
                {
                    throw new Exception($"Số điện thoại {registerAccount.PhoneNumber} đã tồn tại");
                }
                var fileName = "";
                if (registerAccount.Avatar != null)
                {
                    if (string.IsNullOrEmpty(env.WebRootPath))
                    {
                        env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    }
                    var uploadsFolder = Path.Combine(env.WebRootPath, "Images");
                    Directory.CreateDirectory(uploadsFolder);

                    fileName = $"{Guid.NewGuid()}{Path.GetExtension(registerAccount.Avatar.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await registerAccount.Avatar.CopyToAsync(stream);
                    }
                }

                var account = mapper.Map<Account>(registerAccount);
                account.Status = "EMAILNOTACTIVE";
                var hashPawword = passwordHasher.HashPassword(account, account.Password);
                account.Password = hashPawword;
                account.Avatar = $"/Images/{fileName}";

                var newAccount = await accountRepository.AddAccountAsync(account);

                var token = jwtHelper.GenerateEmailToken(newAccount.AccountID.ToString(), newAccount.Username, newAccount.Email, newAccount.PhoneNumber);
                string verificationLink = $"https://localhost:7270/api/Account/verify-email?token={Uri.EscapeDataString(token)}";

                string emailBody =
                      $@"<p>Xin chào {account.Firstname},</p>
                    <p>Vui lòng nhấp vào đường link dưới đây để xác minh email:</p>
                    <p><a href='{verificationLink}'>Xác minh Email</a></p>
                    <p>Liên kết sẽ hết hạn trong 24 giờ.</p>";
                await emailService.SendEmailAsync(account.Email, "Xác minh email", emailBody);

                return newAccount;
            }
        }

        public async Task<List<Account>> GetAllBlankAccountsAsync()
        {
            return await accountRepository.GetAllBlankAccountsAsync();
        }

        public async Task<List<AccountNotation>> GetAllAccountNotationsAsync()
        {
            return await accountRepository.GetAllAccountNotationsAsync();
        }

        public async Task CreateAccountNotation(Account acc, string note)
        {
            var accNote = new AccountNotation
            {
                AccountID = acc.AccountID,
                CreateDate = DateTime.Now,
                Notation = note,
                Processed = false
            };

            await accountRepository.CreateAccountNotationAsync(accNote);
        }


        public async Task<bool> VerifyAccountEmail(int accountId, string username, string email, string phoneNumber)
        {
            var account = await accountRepository.GetAccountByID(accountId);
            if (account == null)
            {
                return false;
            }
            return account.AccountID == accountId
                && account.Username == username
                && account.Email == email
                && account.PhoneNumber == phoneNumber;
        }

        public async Task<Account?> UpdateAccountAsync(UpdateAccountDto updateAccount)
        {
            var email = await accountRepository.GetAccountByEmailAsync(updateAccount.Email);
            if (email != null && email.AccountID != updateAccount.AccountID)
            {
                throw new Exception($"Email {updateAccount.Email} đã tồn tại");
            }
            var phoneNumber = await accountRepository.GetAccountByPhonenumberAsync(updateAccount.PhoneNumber);
            if (phoneNumber != null && phoneNumber.AccountID != updateAccount.AccountID)
            {
                throw new Exception($"Số điện thoại {updateAccount.PhoneNumber} đã tồn tại");
            }
            var fileName = "";
            if (updateAccount.Avatar != null)
            {
                if (string.IsNullOrEmpty(env.WebRootPath))
                {
                    env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                var uploadsFolder = Path.Combine(env.WebRootPath, "Images");
                Directory.CreateDirectory(uploadsFolder);

                fileName = $"{Guid.NewGuid()}{Path.GetExtension(updateAccount.Avatar.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await updateAccount.Avatar.CopyToAsync(stream);
                }
            }
            var tmpAccount = mapper.Map<Account>(updateAccount);
            var hashPawword = passwordHasher.HashPassword(tmpAccount, tmpAccount.Password);
            tmpAccount.Password = hashPawword;
            tmpAccount.Avatar = $"/Images/{fileName}";
            var account = await accountRepository.UpdateAccountAsync(tmpAccount);
            if (account == null)
            {
                throw new Exception($"Tài khoản không tồn tại!");
            }
            return account;
        }

        public async Task<Account?> GetAccountRole(int accountId)
        {
            var account = await accountRepository.GetAccountRole(accountId);
            if (account == null)
            {
                throw new Exception("không tìm thầy tài khoản");
            }

            return account;
        }

        public async Task<List<Account>> GetAllAccountsAsync(FilterAccountDto filterAccount)
        {
            return await accountRepository.GetAllAccountsAsync(filterAccount);
        }

        public async Task<Account?> DisableAccountAsync(int id)
        {
            var account = await accountRepository.GetAccountByID(id);
            if (account == null)
            {
                throw new Exception($"ID {id} không tồn tại");
            }
            if (account.Status == "INACTIVE")
            {
                throw new Exception($"tài khoản đã bị khóa trước đó");
            }
            account.Status = "INACTIVE";
            return await accountRepository.DisableAccountAsync(account);
        }
        public async Task<Account?> EnableAccountAsync(int id)
        {
            var account = await accountRepository.GetAccountByID(id);
            if (account == null)
            {
                throw new Exception($"ID {id} không tồn tại");
            }
            if (account.Status == "ACTIVE")
            {
                throw new Exception($"tài khoản đã được kích hoạt trước đó");
            }
            account.Status = "ACTIVE";
            return await accountRepository.EnableAccountAsync(account);
        }
        

        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            return await accountRepository.GetAccountByID(accountId);
        }


        public async Task<Account?> GetAccountByID(int id)
        {
            return await accountRepository.GetAccountByID(id);
        }

        public async Task<Account?> GetParentByChildIDAsync(int childID)
        {
            return await accountRepository.GetParentByChildIDAsync(childID);
        }

        public async Task<AccountNotation> GetAllAccountNotationAccByIDAsync(int accountID)
        {
            return await accountRepository.GetAllAccountNotationByIDAsync(accountID);
        }

        public async Task SetAccountNotationsAsync(int accountID, bool v)
        {
            var accNote = await GetAllAccountNotationAccByIDAsync(accountID);
            if (accNote == null)
            {
                Console.WriteLine("không tìm thấy Account notation");
                return;
            }
            accNote.Processed = v;
            await accountRepository.UpdateAccountNoteAsync(accNote);
        }






        //public async Task<Account?> DeleteAccountAsync(int id)
        //{
        //    var account = await accountRepository.GetAccountByID(id);
        //    if (account == null)
        //    {
        //        throw new Exception($"ID {id} is not available");
        //    }
        //    account = mapper.Map<Account>(account);
        //    return await accountRepository.DeleteAccountsAsync(account);
        //}
    }
}
