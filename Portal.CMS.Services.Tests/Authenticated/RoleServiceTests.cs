﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Portal.CMS.Entities;
using Portal.CMS.Entities.Entities.Authentication;
using Portal.CMS.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Portal.CMS.Services.Tests.Authenticated
{
    [TestClass]
    public class RoleServiceTests
    {
        #region Dependencies

        private PortalEntityModel _mockContext;

        private UserService _userService;
        private RoleService _roleService;

        [TestInitialize]
        public void Initialise()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();

            _mockContext = new PortalEntityModel(connection);
            _mockContext.Database.CreateIfNotExists();

            _userService = new UserService(_mockContext);
            _roleService = new RoleService(_mockContext, _userService);
        }

        #endregion Dependencies

        #region RoleService.Get

        [TestMethod]
        public void GetUserRoles_ReturnsRoles()
        {
            int? userId = 1;

            _mockContext.Users.AddRange(new List<User>
            {
                new User { UserId = userId.Value, GivenName = "Test", FamilyName = "User", EmailAddress = "Email", Password = "Password", DateAdded = DateTime.Now, DateUpdated = DateTime.Now }
            });

            _mockContext.SaveChanges();

            _mockContext.Roles.AddRange(new List<Role>
            {
                new Role { RoleId = 1, RoleName = "Role 1" },
                new Role { RoleId = 2, RoleName = "Role 2" }
            });

            _mockContext.SaveChanges();

            _mockContext.UserRoles.AddRange(new List<UserRole>
            {
                new UserRole { UserRoleId = 1 , UserId = userId.Value, RoleId = 1 },
                new UserRole { UserRoleId = 1 , UserId = userId.Value, RoleId = 2 },
            });

            _mockContext.SaveChanges();

            var result = _roleService.Get(userId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() > 0);
        }

        [TestMethod]
        public void GetUserRoles_ReturnsCorrectRole()
        {
            int? userId = 1;

            _mockContext.Users.AddRange(new List<User>
            {
                new User { UserId = userId.Value, GivenName = "Test", FamilyName = "User", EmailAddress = "Email", Password = "Password", DateAdded = DateTime.Now, DateUpdated = DateTime.Now }
            });

            _mockContext.SaveChanges();

            _mockContext.Roles.AddRange(new List<Role>
            {
                new Role { RoleId = 1, RoleName = "Role 1" },
                new Role { RoleId = 2, RoleName = "Role 2" }
            });

            _mockContext.SaveChanges();

            var userRoles = new List<UserRole>
            {
                new UserRole { UserRoleId = 1 , UserId = userId.Value, RoleId = 1 },
            };

            _mockContext.UserRoles.AddRange(userRoles);

            _mockContext.SaveChanges();

            var result = _roleService.Get(userId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().RoleName == "Role 1");
        }

        [TestMethod]
        public void GetUserRoles_NullUserReturnsAnonymous()
        {
            var result = _roleService.Get(null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void GetUserRoles_InvalidUserReturnsNoRoles()
        {
            int? userId = 100;

            var result = _roleService.Get(userId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() == 0);
        }

        #endregion RoleService.Get

        #region RoleService.Validate

        [TestMethod]
        public void Validate_Admin_CanAccessEverything()
        {
            var entityRoles = new List<Role> { new Role { RoleId = 2, RoleName = "Other Role" } };
            var userRoles = new List<Role> { new Role { RoleId = 1, RoleName = "Admin" } };

            var result = _roleService.Validate(entityRoles, userRoles);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Validate_Authenticated_CanAccessEmptyRoleSets()
        {
            var entityRoles = new List<Role>();
            var userRoles = new List<Role> { new Role { RoleId = 1, RoleName = "Guest" } };

            var result = _roleService.Validate(entityRoles, userRoles);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Validate_Anonymous_NoAccessToLockedEntity()
        {
            var entityRoles = new List<Role> { new Role { RoleId = 1, RoleName = "Any Role" } };
            var userRoles = new List<Role>();

            var result = _roleService.Validate(entityRoles, userRoles);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Validate_Authenticated_CanAccessEntityWithSameRole()
        {
            var entityRoles = new List<Role> { new Role { RoleId = 1, RoleName = "Role 1" }, new Role { RoleId = 2, RoleName = "Role 2" } };
            var userRoles = new List<Role> { new Role { RoleId = 1, RoleName = "Role 1" } };

            var result = _roleService.Validate(entityRoles, userRoles);

            Assert.IsTrue(result);
        }

        #endregion RoleService.Validate
    }
}