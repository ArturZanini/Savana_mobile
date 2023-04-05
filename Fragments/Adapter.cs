using AndroidX.Fragment.App;
using Java.Lang;
using System.Collections.Generic;

namespace SistemaColeta.Fragments
{
    public class Adapter : FragmentStatePagerAdapter
    {
        #region Private fields
        private List<Fragment> _fragments;
        #endregion

        #region Public methods
        public override int Count => _fragments.Count;
        public Adapter(FragmentManager fm) : base(fm, 0)
        {
            _fragments = new List<Fragment>
            {
                new Checklist(),
                new Survey(),
                //new Collect()
            };
        }
        public override Fragment GetItem(int position)
        {
            return _fragments[position];
        }
        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return position switch
            {
                0 => new String("CHECKLIST"),
                1 => new String("SISTEMA"),
                //2 => new String("COLETA"),
                _ => base.GetPageTitleFormatted(position),
            };
        }
        #endregion
    }
}