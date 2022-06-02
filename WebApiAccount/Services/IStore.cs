using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserAccountsDataBaseWebApi;
using WebApiAccount.Models;
using System.Text.RegularExpressions;

namespace WebApiAccount.Services
{
    public interface IStore<T> : IEnumerable<T>, IAsyncEnumerable<T> where T : Entity
    {
        public Task<T> Get(int id);
        public Task<T> Add(T data);
        public Task<T> Delete(int id);
        public Task<T> Update(T data);
        public Task<IQueryable<T>> ExecuteSql(string sql, params object[] parameters);
    }
    public abstract class StoreBase<T> : IStore<T> where T : Entity
    {
        MainDb _mainDb;
        protected StoreBase(MainDb mainDb)
        {
            _mainDb = mainDb;
        }
        public virtual async Task<T> Delete(int id)
        {
            if (id == 0)
            {
                return null;
            }
            bool res;


            var data = await Get(id);

            if (data == null)
                return null;
            try
            {
                _mainDb.Remove(data);
                await _mainDb.SaveChangesAsync();
                res = true;
            }
            catch
            {
                res = false;
            }

            if (res)
                return data;

            return null;
        }

        public virtual async Task<T> Get(int id)
        {
            if (id == 0)
                return null;
            T res;
            try
            {
                res = await _mainDb.FindAsync<T>(id);
            }
            catch (Exception ex)
            {
                res = null;
            }


            return res;
        }

        public virtual async Task<T> Update(T data)
        {
            if (data == null)
                return null;
            bool res = true;



            try
            {
                _mainDb.Update(data);
                await _mainDb.SaveChangesAsync();
            }
            catch
            {
                res = false;
            }
            if (res)
                return await Get(data.Id);
            return null;
        }

        public virtual async Task<T> Add(T data)
        {
            if (data is null)
                return null;

            T returnRes;
            var added = await _mainDb.AddAsync(data);
            try
            {
                var res = await _mainDb.SaveChangesAsync();
                returnRes = added.Entity;
            }
            catch
            {
                returnRes = null;
            }
            return returnRes;
        }
        public virtual async Task<IQueryable<T>> ExecuteSql(string sql, params object[] parameters)
        {
            var table = _mainDb.GetTableByType<T>() as DbSet<T>;
            if (table is null)
                return null;

            var res = table.FromSqlRaw(sql, parameters);
            await _mainDb.SaveChangesAsync();
            return res;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return (_mainDb.GetTableByType<T>() as DbSet<T>).ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_mainDb.GetTableByType<T>() as DbSet<T>).ToList().GetEnumerator();
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return (_mainDb.GetTableByType<T>() as DbSet<T>).AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);
        }
    }
    #region realization
    public class BillStore : StoreBase<Bill>
    {

        OrderStore _orderStore;
        public BillStore(MainDb mainDb, OrderStore orderStore) : base(mainDb)
        {
            _orderStore = orderStore;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sum">can be minus</param>
        /// <param name="bill"></param>
        /// <returns></returns>
        public virtual async Task<Bill> AddSum(int sum, Bill bill)
        {
            bill = await Get(bill?.Id ?? 0);
            if (bill == null)
                return null;

            bill.Money += sum;

            bill = await Update(bill);



            return bill;

        }
        public virtual async Task<Order> AddOrder(Order order, Bill bill)
        {
            bill = await Get(bill?.Id ?? 0);

            order = await _orderStore.Get(order?.Id ?? 0) ?? await _orderStore.Add(order);


            if (order == null || bill == null || order.TotalSum > bill.Money)
                return null;

            bill.Orders.Add(order);
            bill.Money -= order.TotalSum;


            await Update(bill);


            return order;
        }

    }
    public class AvatarStore : StoreBase<Avatar>
    {
        public AvatarStore(MainDb mainDb) : base(mainDb)
        {

        }

        public virtual async Task<Avatar> AddBytes(byte[] bytes, Avatar avatar)
        {
            avatar = await Get(avatar?.Id ?? 0);
            if (bytes == null || avatar == null || bytes == Array.Empty<byte>())
            {
                return null;
            }
            avatar.Avatars = bytes;


            avatar = await Update(avatar);
            return avatar;
        }
    }
    public class GuildStore : StoreBase<Guild>
    {
        UserStore _userStore;
        BillStore _billStore;
        public GuildStore(MainDb mainDb, UserStore userStore, BillStore billStore) : base(mainDb)
        {
            _userStore = userStore;
            _billStore = billStore;
        }
        public virtual async Task<User> AddUser(User user, Guild guild)
        {
            guild = await Get(guild?.Id ?? 0);
            user = await _userStore.Get(user?.Id ?? 0);
            if (user == null || guild == null)
            {
                return null;
            }
            guild.Users.Add(user);
            await Update(guild);

            return user;
        }
        public virtual async Task<Guild> ChangeName(string name, Guild guild)
        {
            guild = await Get(guild?.Id ?? 0);
            if (guild == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            guild.Name = name;
            guild = await Update(guild);
            return guild;
        }
        public virtual async Task<Bill> AddBill(Bill bill, Guild guild)
        {
            guild = await Get(guild?.Id ?? 0);
            bill = await _billStore.Get(bill?.Id ?? 0) ?? await _billStore.Add(bill);
            if (bill == null || guild == null)
                return null;

            guild.Bill = bill;
            guild.BillId = bill.Id;

            await Update(guild);
            return bill;
        }
    }
    public class OrderStore : StoreBase<Order>
    {
        ProductStore _productStore;
        public OrderStore(MainDb mainDb, ProductStore productStore) : base(mainDb)
        {
            _productStore = productStore;
        }
        public virtual async Task<Product> AddProduct(Order order, Product product)
        {
            order = await Get(order?.Id ?? 0);
            product = await _productStore.Get(product?.Id ?? 0) ?? await _productStore.Add(product);

            if (order == null || product == null)
                return null;

            var order1 = new OrderAndProduct();

            order1.DateTime = DateTime.Now;
            order1.Order = order;
            order1.Product = product;


            order.Products.Add(product);
            order.TotalSum += product.Price;

            order.OrderAndProducts.Add(order1);

            await Update(order);
            return product;
        }

    }
    public class ProductStore : StoreBase<Product>
    {
        public ProductStore(MainDb mainDb) : base(mainDb)
        {

        }
        public virtual async Task<Product> ChangePrice(Product product, int price)
        {
            product = await Get(product?.Id ?? 0);
            if (price < 0 && product != null)
            {
                product.Price = price;
                product = await Update(product);
                return product;
            }
            return null;
        }
        public virtual async Task<Product> ChangeName(Product product, string name)
        {
            product = await Get(product?.Id ?? 0);
            if (!string.IsNullOrWhiteSpace(name) && product != null)
            {
                product.Name = name;
                product = await Update(product);
                return product;
            }
            return null;
        }
    }
    public class ReportStore : StoreBase<Report>
    {
        public ReportStore(MainDb mainDb) : base(mainDb)
        {

        }
        public virtual async Task<Report> ChangeDate(DateTime date, Report report)
        {
            report = await Get(report?.Id ?? 0);
            if (report != null)
            {
                report.Date = date;
                report = await Update(report);
                return report;
            }
            return null;
        }
        public virtual async Task<Report> ChangeText(string text, Report report)
        {
            report = await Get(report?.Id ?? 0);
            if (string.IsNullOrWhiteSpace(text) && report != null)
            {
                report.Text = text;
                report = await Update(report);
                return report;
            }
            return null;
        }
    }
    public class UserStore : StoreBase<User>
    {


        AvatarStore _avatarStore;
        BillStore _billStore;
        ReportStore _reportStore;
        //GuildStore _guildStore;
        public UserStore(MainDb mainDb, AvatarStore avatarStore, BillStore bills, ReportStore reports) : base(mainDb)
        {
            _avatarStore = avatarStore;
            _billStore = bills;
            _reportStore = reports;
            //_guildStore = guilds;
        }


        public virtual async Task<Avatar> AddAvatar(User user, Avatar avatar)
        {
            user = await Get(user?.Id ?? 0);
            avatar = await _avatarStore.Get(avatar?.Id ?? 0) ?? await _avatarStore.Add(avatar);
            if (user == null || avatar == null)
                return null;



            user.Avatar = avatar;
            user.AvatarId = avatar.Id;
            avatar.User = user;
            avatar.UserId = user.Id;
            user = await Update(user);
            return avatar;
        }
        public virtual async Task<Bill> AddBill(User user, Bill bill)
        {
            user = await Get(user?.Id ?? 0);
            bill = await _billStore.Get(bill?.Id ?? 0) ?? await _billStore.Add(bill);
            if (user == null || bill == null)
                return null;

            user.Bill = bill;
            user.BillId = bill.Id;
            user = await Update(user);

            return bill;
        }
        public virtual async Task<Report> AddReport(User user, Report report)
        {
            user = await Get(user?.Id ?? 0);
            report = await _reportStore.Get(report?.Id ?? 0) ?? await _reportStore.Add(report);
            if (user == null || report == null)
                return null;

            user.Reports.Add(report);
            user = await Update(user);

            return report;
        }
        //public virtual async Task<bool> AddToGuild(User user, Guild guild)
        //{
        //    if (user == null || guild == null || await Get(user.Id) == null || await _guildStore.Get(guild.Id) == null)
        //    {
        //        return false;
        //    }


        //    user.Guild = guild;
        //    user.Id = guild.Id;
        //    return await Update(user);

        //}
        public virtual async Task<User> ChangePassword(User user, string password, string oldPassword)
        {

            user = await Get(user?.Id ?? 0);
            if (user == null || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(oldPassword)
                || User.GetHashCode(oldPassword) != user.Password
                )
            {
                return null;
            }



            user.Password = User.GetHashCode(password);
            user = await Update(user);
            return user;

        }
        public virtual async Task<User> ChangeNickname(User user, string nickname)
        {
            user = await Get(user?.Id ?? 0);
            if (user != null && !string.IsNullOrWhiteSpace(nickname))
            {
                user.Nick = nickname;
                user = await Update(user);
                return user;
            }
            return null;
        }

    }
    #endregion


    public abstract class ValidationBase<T>
    {
        public abstract bool Validate(T data);
        public ValidationResult<T> ValidationResult { get; protected set; } = new ValidationResult<T>();
    }
    public abstract class ValidationEntityBase<T> : ValidationBase<T>
    {
        public T Entity { get; set; }
    }
    #region realization
    public class AvatarValidation : ValidationEntityBase<Avatar>
    {
        const long maxLenth = 1048576;
        string[] allowedExts = new string[] {"jpg", "png" };

        public override bool Validate(Avatar enity)
        {
            Entity = enity;
            
            
            
            if (enity.Avatars.Length > maxLenth)
            {

                ValidationResult.IsValidate = false;
                ValidationResult.Entity = enity;
                ValidationResult.Messages.Add(new ValidationMessage {DataType = "bytes/imges", Message = $"maxLength is {maxLenth}b" });
            }
            

            return ValidationResult.IsValidate;
        }
    }
    public class UserValidation : ValidationEntityBase<User>
    {
        private void RegularExp(string strReg, string str,ValidationMessage message, bool isValidIfNotMatch = false)
        {
            if(str is null && string.IsNullOrWhiteSpace(str))
            {
                ValidationResult.Messages.Add(new ValidationMessage {DataType = "User/"+str, Message = "is nul or white spaces", Place = "UserValidation" });
                ValidationResult.IsValidate = false;
            }

            Regex regex = new Regex(strReg);
            var isMatch = regex.IsMatch(str);
            if (!isMatch)
            {
                ValidationResult.Messages.Add(message);
                ValidationResult.IsValidate = isValidIfNotMatch;
            }
        }
        public virtual bool ValidatePassword(string password)
        {   
            

            RegularExp(@"^.{5,20}$", password, new ValidationMessage { DataType = "User/password", Message = "regex is not matched. Max length password is 20. Min length password is 5", Place = "UserValidation" });



            return ValidationResult.IsValidate;
        }
        public virtual bool ValidateNickname(string nick)
        {
            RegularExp(@"^[A-Za-z]{5,20}$", nick, new ValidationMessage { DataType = "User/nick", Message = "regex is not matched. Max length password is 20. Min length password is 5. Syms can be only litter", Place = "UserValidation" });
            
            return ValidationResult.IsValidate;
        }
        public override bool Validate(User entity)
        {
            Entity = entity;
            ValidatePassword(entity.StrPassword);
            ValidateNickname(entity.Nick);
            if (Entity.Password == 0)
            {
                ValidationResult.IsValidate = false;
                ValidationResult.Messages.Add(new ValidationMessage {DataType = "User/pasword-hash", Message = "hash is 0", Place = "User/Password" });
            }

            return ValidationResult.IsValidate;
        }
    }
    public class BillValidation : ValidationEntityBase<Bill>
    {

        public override bool Validate(Bill enity)
        {
            Entity = enity;
            

            return ValidationResult.IsValidate;
        }
    }
    public class GuildValidation : ValidationEntityBase<Guild>
    {
        private void RegularExp(string strReg, string str, ValidationMessage message, bool isValidIfNotMatch = false)
        {
            if (str is null && string.IsNullOrWhiteSpace(str))
            {
                ValidationResult.Messages.Add(new ValidationMessage { DataType = "Guild/" + str, Message = "is nul or white spaces", Place = "UserValidation" });
                ValidationResult.IsValidate = false;
            }

            Regex regex = new Regex(strReg);
            var isMatch = regex.IsMatch(str);
            if (!isMatch)
            {
                ValidationResult.Messages.Add(message);
                ValidationResult.IsValidate = isValidIfNotMatch;
            }
        }

        public override bool Validate(Guild enity)
        {
            Entity = enity;
            ValidateName(Entity.Name);
            return ValidationResult.IsValidate;
        }
        public virtual bool ValidateName(string name)
        {
            RegularExp(@"^[A-Za-z]{5,20}$", name, new ValidationMessage { DataType = "Guild/name", Message = "regex is not matched. Max length  is 20. Min length password is 5. Syms can be only litter", Place = "GuildValidation" });
            return ValidationResult.IsValidate;
        }
    }
    public class OrderValidation : ValidationEntityBase<Order>
    {


        public override bool Validate(Order enity)
        {
            Entity = enity;

            if (Entity.TotalSum < 0)
            {
                ValidationResult.Messages.Add(new ValidationMessage {DataType = "Order", Message = "total sum cannot be less 0" });
                ValidationResult.IsValidate = false;
            }

            return ValidationResult.IsValidate;
        }
    }
    public class ProductValidation : ValidationEntityBase<Product>
    {
        private void RegularExp(string strReg, string str, ValidationMessage message, bool isValidIfNotMatch = false)
        {
            if (str is null && string.IsNullOrWhiteSpace(str))
            {
                ValidationResult.Messages.Add(new ValidationMessage { DataType = "User/" + str, Message = "is nul or white spaces", Place = "UserValidation" });
                ValidationResult.IsValidate = false;
            }

            Regex regex = new Regex(strReg);
            var isMatch = regex.IsMatch(str);
            if (!isMatch)
            {
                ValidationResult.Messages.Add(message);
                ValidationResult.IsValidate = isValidIfNotMatch;
            }
        }
        public virtual bool ValidateName(string name)
        {
            RegularExp(@"^[A-Za-z]{1,100}$", name, new ValidationMessage { DataType = "Product/nick", Message = "regex is not matched. Max length password is 100. Min length is 1. Syms can be only litter", Place = "ProductValidation" });
            return ValidationResult.IsValidate;
        }
        public override bool Validate(Product enity)
        {
            Entity = enity;
            ValidateName(Entity.Name);
            if (Entity.Price < 0)
            {
                ValidationResult.Messages.Add(new ValidationMessage { DataType = "Product/price", Message = "price cannot be less 0", Place = "ProductValidation" });
                ValidationResult.IsValidate = false;
            }

            return ValidationResult.IsValidate;
        }
    }
    public class ReportValidation : ValidationEntityBase<Report>
    {
        private void RegularExp(string strReg, string str, ValidationMessage message, bool isValidIfNotMatch = false)
        {
            if (str is null && string.IsNullOrWhiteSpace(str))
            {
                ValidationResult.Messages.Add(new ValidationMessage { DataType = "Report/" + str, Message = "is nul or white spaces", Place = "UserValidation" });
                ValidationResult.IsValidate = false;
            }

            Regex regex = new Regex(strReg);
            var isMatch = regex.IsMatch(str);
            if (!isMatch)
            {
                ValidationResult.Messages.Add(message);
                ValidationResult.IsValidate = isValidIfNotMatch;
            }
        }
        public virtual bool ValidateDateTime(DateTime dateTime)
        {
            return ValidationResult.IsValidate;
        }
        public virtual bool ValidateDateText(string text)
        {
            RegularExp(@"^.{1,500}$", text, new ValidationMessage { DataType = "Report/text", Message = "max length is 500", Place = "UserValidation" });
            return ValidationResult.IsValidate;
        }
        public override bool Validate(Report enity)
        {
            Entity = enity;
            ValidateDateText(Entity.Text);
            ValidateDateTime(Entity.Date);
            return ValidationResult.IsValidate;
        }
    }
    #endregion
    public class ValidationResult<T>
    {
        public List<ValidationMessage> Messages { get; set; } = new List<ValidationMessage>();
        public bool IsValidate { get; set; } = true;
        public T Entity { get; set; }
    }
    public class ValidationMessage
    {
        public string Message { get; set; }
        public string DataType { get; set; }
        public string Place { get; set; }
    }
    public abstract class StoreAgentBase<DataType, StoreType> : IEnumerable<DataType>, IAsyncEnumerable<DataType> where DataType : Entity where StoreType : IStore<DataType>
    {
        public StoreType Store { protected get; set; }
        public ValidationEntityBase<DataType> Validator { get; set; }

        protected StoreAgentBase(StoreType store, ValidationEntityBase<DataType> validator)
        {
            Store = store;
            Validator = validator;
        }
        protected StoreAgentBase(StoreType store)
        {
            Store = store;
        }
        public virtual async Task<ValidationResult<DataType>> Get(int id)
        {
            var valid = new ValidationResult<DataType>();
            var task = Store.Get(id);
            var entity = await task;
            valid.Entity = entity;
            if (entity is null)
            {
                valid.IsValidate = false;
                valid.Messages.Add(new ValidationMessage { DataType = typeof(DataType).Name, Message = "entity not found or something went wrong" });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(DataType).Name, Message = task?.Exception?.Message });
            }
            else
            {
                valid.IsValidate = true;
            }
            return valid;
        }
        public virtual async Task<ValidationResult<DataType>> Add(DataType data)
        {
            ValidationResult<DataType> valid = Validator.ValidationResult;
            if (Validator.Validate(data))
            {
                var task = Store.Add(data);
                var res = await task;

                if (res == null)
                {
                    valid.IsValidate = false;
                    valid.Entity = res;
                    valid.Messages.Add(new ValidationMessage { DataType = typeof(DataType).Name, Message = "Result is null. Not valid or something went wrong", Place = this.ToString() });
                    valid.Messages.Add(new ValidationMessage { DataType = typeof(DataType).Name, Message = task?.Exception?.Message, Place = this.ToString() });
                }
                else
                {
                    valid.Entity = res;
                    valid.IsValidate = true;
                }
            }
            return valid;
        }
        public virtual async Task<ValidationResult<DataType>> Delete(int id)
        {

            var valid = new ValidationResult<DataType>();
            var task = Store.Delete(id);
            var res = await task;
            valid.Entity = res;
            if (res == null)
            {
                valid.IsValidate = false;
                valid.Messages.Add(new ValidationMessage { DataType = typeof(DataType).Name, Message = "entity not found or something went wrong" });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(DataType).Name, Message = task?.Exception?.Message });
            }
            else
            {
                valid.IsValidate = true;
            }
            return valid;

        }

        public IEnumerator<DataType> GetEnumerator()
        {
            return Store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Store.GetEnumerator();
        }

        public IAsyncEnumerator<DataType> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return Store.GetAsyncEnumerator(cancellationToken);
        }
    }
    #region realization
    public class UsersAgent : StoreAgentBase<User, UserStore>
    {
        UserStore _userStore;
        AvatarValidation _avatarValidation;
        BillValidation _billValidation;
        ReportValidation _reportValidation;
        public UsersAgent(UserStore userStore, UserValidation validator, AvatarValidation avatarValidation, BillValidation billValidation, ReportValidation reportValidation) : base(userStore, validator)
        {
            _userStore = userStore;
            _avatarValidation = avatarValidation;
            _billValidation = billValidation;
            _reportValidation = reportValidation;
        }
        public virtual async Task<ValidationResult<Avatar>> AddAvatar(User user, Avatar avatar)
        {
            ValidationResult<Avatar> validationResult = _avatarValidation.ValidationResult;
            if (_avatarValidation.Validate(avatar))
            {
                var task = _userStore.AddAvatar(user, avatar);
                var res = await task;

                validationResult.IsValidate = res != null && !task.IsFaulted;
                validationResult.Entity = res;

                if (validationResult.IsValidate == false)
                {
                    validationResult.Messages.Add(new ValidationMessage { DataType = typeof(Avatar).Name, Place = this.ToString(), Message = "Not valid.It would be exeption" });
                    validationResult.Messages.Add(new ValidationMessage { DataType = typeof(Avatar).Name, Place = "Inner exeption", Message = task.Exception?.Message });


                }
            }
            return validationResult;

        }
        public virtual async Task<ValidationResult<Bill>> AddBill(User user, Bill bill)
        {
            ValidationResult<Bill> validationResult;
            validationResult = _billValidation.ValidationResult;
            if (_billValidation.Validate(bill))
            {
                var task = _userStore.AddBill(user, bill);
                var res = await task;
                validationResult.Entity = res;
                validationResult.IsValidate = res != null && !task.IsFaulted;
                if (!validationResult.IsValidate)
                {
                    validationResult.Messages.Add(new ValidationMessage { DataType = typeof(Bill).Name, Message = "Not valid", Place = this.ToString() });
                    validationResult.Messages.Add(new ValidationMessage { DataType = typeof(Bill).Name, Place = "Inner exeption", Message = task.Exception?.Message });
                }

            }
            return validationResult;
        }
        public virtual async Task<ValidationResult<Report>> AddReport(User user, Report report)
        {
            ValidationResult<Report> validation;
            validation = _reportValidation.ValidationResult;
            if (_reportValidation.Validate(report))
            {
                var task = _userStore.AddReport(user, report);
                var res = await task;

                validation.IsValidate = res != null;
                validation.Entity = res;

                if (!validation.IsValidate)
                {
                    validation.Messages.Add(new ValidationMessage { DataType = typeof(Report).Name, Message = "is not valid", Place = this.ToString() });
                    validation.Messages.Add(new ValidationMessage { DataType = typeof(Report).Name, Message = task.Exception?.Message, Place = "Inner exeption" });
                }



            }
            return validation;
        }
        //public virtual async Task<ValidationResult<Guild>> AddToGuild(User user, Guild guild)
        //{
        //    var valid = _guildValidation.ValidationResult;

        //    if (_guildValidation.Validate(guild))
        //    {
        //        var res = await _userStore.AddToGuild(user, guild);
        //        valid.IsValidate = res;
        //        if (!res)
        //            valid.Messages.Add(new ValidationMessage { DataType = typeof(Guild), Message = "is not valid", Place = this });
        //    }
        //    return valid;
        //}
        public virtual async Task<ValidationResult<User>> ChangePassword(User user, string password, string oldPassword)
        {
            var valid = (UserValidation)Validator;
            var validRes = valid.ValidationResult;
            User res = null;
            Task<User> task = null;
            if (valid.ValidatePassword(password))
            {
                task = _userStore.ChangePassword(user, password, oldPassword);
                res = await task;
            }

            validRes.IsValidate = res != null;
            validRes.Entity = res;

            if (!validRes.IsValidate)
            {
                validRes.Messages.Add(new ValidationMessage { DataType = typeof(User).Name, Message = "Not valid", Place = this.ToString() });
                validRes.Messages.Add(new ValidationMessage { DataType = typeof(User).Name, Message = task?.Exception?.Message, Place = "Inner exeption" });
            }

            return validRes;
        }
        public virtual async Task<ValidationResult<User>> ChangeNickname(User user, string nickname)
        {
            var validator = (UserValidation)Validator;
            var validRes = validator.ValidationResult;

            User res = null;
            Task<User> task = null;
            if (validator.ValidateNickname(nickname))
            {
                task = Store.ChangeNickname(user, nickname);
                res = await task;
            }

            validRes.IsValidate = res != null;
            validRes.Entity = res;

            if (!validRes.IsValidate)
            {
                validRes.Messages.Add(new ValidationMessage { DataType = typeof(User).Name, Message = "is not valid", Place = this.ToString() });
                validRes.Messages.Add(new ValidationMessage { DataType = typeof(User).Name, Message = task?.Exception?.Message, Place = "Inner exeption" });
            }

            return validRes;
        }
    }
    public class AvatarsAgent : StoreAgentBase<Avatar, AvatarStore>
    {
        public AvatarsAgent(AvatarStore avatarStore, AvatarValidation validator) : base(avatarStore, validator)
        {

        }

    }
    public class BillsAgent : StoreAgentBase<Bill, BillStore>
    {
        BillStore _billStore;
        OrderValidation _orderValidation;
        public BillsAgent(BillStore billStore, BillValidation validator, OrderValidation orderValidation) : base(billStore, validator)
        {
            _billStore = billStore;
            _orderValidation = orderValidation;
        }
        public virtual async Task<ValidationResult<Order>> AddOrder(Bill bill, Order order)
        {
            var valid = _orderValidation.ValidationResult;

            Task<Order> task = null;
            Order res = null;
            if (_orderValidation.Validate(order))
            {
                task = _billStore.AddOrder(order, bill);
                res = await task;
            }

            valid.IsValidate = res != null;
            valid.Entity = res;

            if (!valid.IsValidate)
            {
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Order).Name, Message = "is not valid", Place = this.ToString() });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Order).Name, Message = task?.Exception?.Message, Place = "Inner exeption" });
            }

            return valid;
        }
        public virtual async Task<ValidationResult<Bill>> AddSum(int sum, Bill bill)
        {
            var validator = (BillValidation)Validator;
            var valid = validator.ValidationResult;

            var task = _billStore.AddSum(sum, bill);
            var res = await task;
            valid.IsValidate = res != null;
            valid.Entity = res;

            if (!valid.IsValidate)
            {
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Bill).Name, Message = "is not valid", Place = this.ToString() });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Bill).Name, Message = task?.Exception?.Message, Place = "Inner exeption" });
            }

            return valid;
        }
    }
    public class GuildsAgent : StoreAgentBase<Guild, GuildStore>
    {
        BillValidation _billValidation;
        UserValidation _userValidation;
        public GuildsAgent(GuildStore store, GuildValidation validator, BillValidation billValidation, UserValidation userValidation) : base(store, validator)
        {
            _billValidation = billValidation;
            _userValidation = userValidation;
        }
        public virtual async Task<ValidationResult<Bill>> AddBill(Guild guild, Bill billData)
        {
            var valid = _billValidation.ValidationResult;
            Bill bill = null;
            Task<Bill> task = null;
            if (_billValidation.Validate(billData))
            {
                task = Store.AddBill(billData, guild);
                bill = await task;

            }
            valid.Entity = bill;
            valid.IsValidate = bill != null;

            if (!valid.IsValidate)
            {
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Bill).Name, Message = "is not valid", Place = this.ToString() });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Bill).Name, Message = task?.Exception?.Message, Place = "Inner exeption" });
            }

            return valid;
        }
        public virtual async Task<ValidationResult<User>> AddUser(Guild guild, User user)
        {
            var validator = _userValidation;
            var valid = validator.ValidationResult;

            var task = Store.AddUser(user, guild);
            var res = await task;
            valid.IsValidate = res != null;
            valid.Entity = res;

            if (!valid.IsValidate)
            {
                valid.Messages.Add(new ValidationMessage { DataType = typeof(User).Name, Message = "Not valid", Place = this.ToString() });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(User).Name, Message = task?.Exception?.Message, Place = "Inner exeption" });
            }

            return valid;
        }
        public virtual async Task<ValidationResult<Guild>> ChangeName(string name, Guild guild)
        {
            var validator = (GuildValidation)Validator;
            var valid = validator.ValidationResult;
            Guild res = null;
            Task<Guild> task = null;
            if (validator.ValidateName(name))
            {
                task = Store.ChangeName(name, guild);
                res = await task;
            }

            valid.IsValidate = res != null;
            valid.Entity = res;

            if (!valid.IsValidate)
            {
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Guild).Name, Message = "is not valid", Place = this.ToString() });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Guild).Name, Message = task?.Exception?.Message, Place = "Inner exeption" });
            }

            return valid;
        }
    }
    public class OrdersAgent : StoreAgentBase<Order, OrderStore>
    {
        ProductValidation _productValidation;
        public OrdersAgent(OrderStore store, OrderValidation validator, ProductValidation productValidation) : base(store, validator)
        {
            _productValidation = productValidation;
        }
        public async Task<ValidationResult<Product>> AddProduct(Order order, Product product)
        {
            Product res = null;
            var valid = _productValidation.ValidationResult;
            Task<Product> task = null;
            if (_productValidation.Validate(product))
            {
                task = Store.AddProduct(order, product);
                res = await task;
            }


            valid.IsValidate = res != null;
            valid.Entity = res;

            if (!valid.IsValidate)
            {
                valid.Messages.Add(new ValidationMessage { Message = "is not valid", DataType = typeof(Product).Name, Place = this.ToString() });
                valid.Messages.Add(new ValidationMessage { Message = task?.Exception?.Message, DataType = typeof(Product).Name, Place = "Inner exception" });
            }

            return valid;
        }
    }
    public class ProductsAgent : StoreAgentBase<Product, ProductStore>
    {
        public ProductsAgent(ProductStore store, ProductValidation validator) : base(store, validator)
        {

        }
        public virtual async Task<ValidationResult<Product>> ChangePrice(int price, Product product)
        {
            var validator = (ProductValidation)Validator;
            var valid = validator.ValidationResult;
            var task = Store.ChangePrice(product, price);
            var res = await task;

            valid.IsValidate = res != null;
            valid.Entity = res;

            if (!valid.IsValidate)
            {
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Product).Name, Message = "is not valid", Place = this.ToString() });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Product).Name, Message = task?.Exception?.Message, Place = "Inner exception" });
            }


            return valid;
        }
        public virtual async Task<ValidationResult<Product>> ChangeName(string name, Product product)
        {
            var validator = (ProductValidation)Validator;
            var valid = validator.ValidationResult;
            var task = Store.ChangeName(product, name);
            var res = await task;

            valid.IsValidate = res != null;
            valid.Entity = res;

            if (!valid.IsValidate)
            {
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Product).Name, Message = "is not valid", Place = this.ToString() });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Product).Name, Message = task?.Exception?.Message, Place = "Inner exception" });
            }
            return valid;
        }
    }
    public class ReportsAgent : StoreAgentBase<Report, ReportStore>
    {
        public ReportsAgent(ReportStore store, ReportValidation validator) : base(store, validator)
        {

        }
        public virtual async Task<ValidationResult<Report>> ChangeDate(DateTime date, Report report)
        {
            var validor = (ReportValidation)Validator;
            var valid = validor.ValidationResult;
            Report res = null;
            Task<Report> task = null;
            if (validor.ValidateDateTime(date))
            {
                task = Store.ChangeDate(date, report);
                res = await task;
            }

            valid.IsValidate = res != null;
            valid.Entity = res;

            if (!valid.IsValidate)
            {
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Report).Name, Message = "is not valid", Place = this.ToString() });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Report).Name, Message = task?.Exception?.Message, Place = "Inner exception" });
            }
            return valid;
        }
        public virtual async Task<ValidationResult<Report>> ChangeText(string text, Report report)
        {
            var validator = (ReportValidation)Validator;
            var valid = validator.ValidationResult;
            Report res = null;
            Task<Report> task = null;
            if (validator.ValidateDateText(text))
            {
                task = Store.ChangeText(text, report);
                res = await task;
            }


            valid.IsValidate = res != null;
            valid.Entity = res;

            if (!valid.IsValidate)
            {
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Report).Name, Message = "is not valid", Place = this.ToString() });
                valid.Messages.Add(new ValidationMessage { DataType = typeof(Report).Name, Message = task?.Exception?.Message, Place = "Inner exception" });
            }

            return valid;
        }
    }
    #endregion
}
