using Coldairarrow.Business.Cache;
using Coldairarrow.Entity.Base_Manage;
using Coldairarrow.IBusiness;
using Coldairarrow.Util;
using EFCore.Sharding;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Coldairarrow.Business.Base_Manage
{
    public class HomeBusiness : BaseBusiness<Base_User>, IHomeBusiness, ITransientDependency
    {
        private readonly IBase_UserCache _base_UserCache;
        public HomeBusiness(IDbAccessor db, IBase_UserCache base_UserCache, IOperator @operator = null)
            : base(db, @operator)
        {
            _base_UserCache = base_UserCache;
        }

        public async Task<string> SubmitLoginAsync(LoginInputDTO input)
        {
            input.password = input.password.ToMD5String();
            var theUser = await GetIQueryable()
                .Where(x => x.UserName == input.userName && x.Password == input.password)
                .FirstOrDefaultAsync();

            if (theUser.IsNullOrEmpty())
                throw new BusException("账号或密码不正确！");

            return theUser.Id;
        }

        public async Task ChangePwdAsync(ChangePwdInputDTO input)
        {
            var theUser = _operator.Property;
            if (theUser.Password != input.oldPwd?.ToMD5String())
                throw new BusException("原密码错误!");

            theUser.Password = input.newPwd.ToMD5String();
            await UpdateAsync(theUser.Adapt<Base_User>());

            //更新缓存
            await _base_UserCache.UpdateCacheAsync(theUser.Id);
        }
    }
}
