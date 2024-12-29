using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UserCompleteController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly ReusableSql _reusableSql;
        public UserCompleteController(IConfiguration config)
        {
            _dapper = new(config);
            _reusableSql = new(config);
        }


        [HttpGet("GetUsers/{userId}/{isActive}")]
        public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
        {
            string sql = @"EXEC TutorialAppSchema.spUsers_Get";
            string stringParameters = "";

            DynamicParameters sqlParameters = new();

            if (userId != 0)
            {
                stringParameters += ", @UserId=@UserIdParameter";
                sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
            }

            stringParameters += ", @Active=@ActiveParameter";
            sqlParameters.Add("@ActiveParameter", isActive, DbType.Boolean);

            if (stringParameters.Length > 0)
            {
                sql += stringParameters.Substring(1);
            }

            IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(sql, sqlParameters);
            return users;
        }

        [HttpPut("UpsertUser")]
        public IActionResult UpsertUser(UserComplete user)
        {
            if (_reusableSql.UpsertUser(user))
            {
                return Ok();
            }

            throw new Exception("Failed to update user");
        }


        [HttpDelete("DeleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            string sql = @"TutorialAppSchema.spUser_Delete
                @UserId = @UserIdParameter";

            DynamicParameters sqlParameters = new();
            sqlParameters.Add("UserIdParameter", userId, DbType.Int32);

            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to delete user");
        }
    }
}

