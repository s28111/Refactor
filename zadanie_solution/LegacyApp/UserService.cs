using System;
using LegacyAppTests;

namespace LegacyApp
{
    public interface ICreditLimitSrevice
    {
        int GetCreditLimit(string lastName, DateTime birthday);
    }
    public interface IClientRepository
    {
        Client GetById(int idClient);
    }
    public class UserService
    {
        private IClientRepository _clientRepository;
        private ICreditLimitSrevice _creditLimitSrevice;

        public UserService()
        {
            _clientRepository = new FakeClientRepository();
            _creditLimitSrevice = new FakeUserCredit();
        }

        public UserService(IClientRepository clientRepository, ICreditLimitSrevice creditLimitSrevice)
        {
            _clientRepository = clientRepository;
            _creditLimitSrevice = creditLimitSrevice;
        }
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!ValidateInformations(firstName, lastName, email, dateOfBirth))
            {
                return false;
            }

            var user = CreateUser(firstName, lastName, email, dateOfBirth, clientId);
            
            SetCredit(user);
          
            
            return AddUserToDataBase(user);
        }

        public bool ValidateInformations(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            // Logika biznesowa - walidacja
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }
            // Logika biznesowa - walidacja
            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }
            // Logika biznesowa
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21)
            {
                return false;
            }

            return true;
        }

        public User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            //Infrastruktura
            var client = _clientRepository.GetById(clientId);
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
            return user;
        }

        public void SetCredit(User user)
        {
            var clientType = user.Client.GetType();
            //Logika biznesowa + infrastruktura
            if (clientType.Equals("VeryImportantClient"))
            {
                user.HasCreditLimit = false;
            }
            else if (clientType.Equals("ImportantClient"))
            {
                int creditLimit = _creditLimitSrevice.GetCreditLimit(user.LastName, user.DateOfBirth);
                creditLimit = creditLimit * 2;
                user.CreditLimit = creditLimit;
                
            }
            else
            {
                user.HasCreditLimit = true;
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }
        }

        public bool AddUserToDataBase(User user)
        {
            //Logika biznesowa
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }
            //Infrastruktura
            UserDataAccess.AddUser(user);
            return true;
        }
    }
}
