using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateUser(CreateUserInput input)
        {
            return base.CreateUser(input);
        }

        public override Empty EditUser(EditUserInput input)
        {
            return base.EditUser(input);
        }

        public override Empty DeleteUser(DeleteUserInput input)
        {
            return base.DeleteUser(input);
        }

        public override UserList GetUserList(GetUserListInput input)
        {
            return base.GetUserList(input);
        }
    }
}