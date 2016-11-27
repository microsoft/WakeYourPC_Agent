using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WakeYourPC.WakeUpAgent
{
    public static class UserEntitySingletonFactory
    {
        private static UserEntity _currentUserEntity = null;

        public static UserEntity CreateUserEntity(string userName)
        {
            if(_currentUserEntity == null && !string.IsNullOrEmpty(userName))
            {
                _currentUserEntity = new UserEntity(userName);
            }

            return _currentUserEntity;
        }

        public static UserEntity GetInstance()
        {
            return _currentUserEntity;
        }
    }

    public class UserEntity
    {
        /// <summary>
        /// Gets or sets the User name for the customer.
        /// A property for use in Table storage must be a public property of a 
        /// supported type that exposes both a getter and a setter.        
        /// </summary>
        /// <value>
        /// The User name.
        /// </value>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password of the customer.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        public UserEntity(string userName)
        {
            this.Username = userName;
        }
    }
}
