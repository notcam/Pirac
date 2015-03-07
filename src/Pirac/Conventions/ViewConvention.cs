using Conventional.Conventions;

namespace Pirac.Conventions
{
    public class ViewConvention : Convention
    {
        public ViewConvention()
        {
            Must.HaveNameEndWith("View");

            BaseName = t => t.Name.Substring(0, t.Name.Length - 4);

            Variants.HaveBaseNameAndEndWith("View");
        }
    }
}