using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Tools
{
    public interface IChooser : IEnumerable
    {
        object Choose();
    }
}