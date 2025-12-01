using Coldairarrow.Entity;
using Coldairarrow.Entity.Base_Manage;
using Coldairarrow.IBusiness;
using Coldairarrow.Util;
using EFCore.Sharding;
using LinqKit;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Coldairarrow.Business.Base_Manage
{
    public class Base_RoleBusiness : BaseBusiness<Base_Role>, IBase_RoleBusiness, ITransientDependency
    {
        public Base_RoleBusiness(IDbAccessor db, IOperator @operator = null)
            : base(db, @operator)
        {
        }

        #region 外部接口

        public async Task<PageResult<Base_RoleInfoDTO>> GetDataListAsync(PageInput<RolesInputDTO> input)
        {
            var where = LinqHelper.True<Base_Role>();
            var search = input.Search;
            if (!search.roleId.IsNullOrEmpty())
                where = where.And(x => x.Id == search.roleId);
            if (!search.roleName.IsNullOrEmpty())
                where = where.And(x => x.RoleName.Contains(search.roleName));

            // 查询原始实体并分页
            var pageQuery = GetIQueryable().Where(where);
            var pageResult = await pageQuery.ProjectToType<Base_RoleInfoDTO>().GetPageResultAsync(input);

            await SetProperty(pageResult.Data);

            return pageResult;

            async Task SetProperty(List<Base_RoleInfoDTO> _list)
            {
                var allActionIds = await Db.GetIQueryable<Base_Action>().Select(x => x.Id).ToListAsync();

                var ids = _list.Select(x => x.Id).ToList();
                var roleActions = await Db.GetIQueryable<Base_RoleAction>()
                    .Where(x => ids.Contains(x.RoleId))
                    .ToListAsync();
                _list.ForEach(aData =>
                {
                    if (aData.RoleName == RoleTypes.超级管理员.ToString())
                        aData.Actions = allActionIds;
                    else
                        aData.Actions = roleActions.Where(x => x.RoleId == aData.Id).Select(x => x.ActionId).ToList();
                });
            }
        }

        public async Task<Base_RoleInfoDTO> GetTheDataAsync(string id)
        {
            return (await GetDataListAsync(new PageInput<RolesInputDTO> { Search = new RolesInputDTO { roleId = id } })).Data.FirstOrDefault();
        }

        [DataAddLog(UserLogType.系统角色管理, "RoleName", "角色")]
        [DataRepeatValidate(new string[] { "RoleName" }, new string[] { "角色名" })]
        public async Task AddDataAsync(Base_RoleInfoDTO input)
        {
            await InsertAsync(input.Adapt<Base_Role>());
            await SetRoleActionAsync(input.Id, input.Actions);
        }

        [DataEditLog(UserLogType.系统角色管理, "RoleName", "角色")]
        [DataRepeatValidate(new string[] { "RoleName" }, new string[] { "角色名" })]
        [Transactional]
        public async Task UpdateDataAsync(Base_RoleInfoDTO input)
        {
            await UpdateAsync(input.Adapt<Base_Role>());
            await SetRoleActionAsync(input.Id, input.Actions);
        }

        [DataDeleteLog(UserLogType.系统角色管理, "RoleName", "角色")]
        [Transactional]
        public async Task DeleteDataAsync(List<string> ids)
        {
            await DeleteAsync(ids);
            await Db.DeleteAsync<Base_RoleAction>(x => ids.Contains(x.RoleId));
        }

        #endregion

        #region 私有成员

        private async Task SetRoleActionAsync(string roleId, List<string> actions)
        {
            var roleActions = (actions ?? new List<string>())
                .Select(x => new Base_RoleAction
                {
                    ActionId = x,
                    RoleId = roleId
                }).ToList();
            await Db.DeleteAsync<Base_RoleAction>(x => x.RoleId == roleId);
			await InsertEntityAsync(roleActions);
        }

        #endregion

        #region 数据模型

        #endregion
    }
}