using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.ViewPager.Widget;
using Google.Android.Material.Tabs;
using System;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace SistemaColeta.Activities
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorPortrait)]
    public class CollectPointActivity : BaseActivity
    {
        #region Private fields
        private int CurrentId => Convert.ToInt32(Intent.GetStringExtra("Id"));
        #endregion

        #region Protected methods
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_collect_point);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            Title = $"{GetString(Resource.String.collect_point)} {CurrentId}";

            Fragments.Adapter fragmentAdapter = new Fragments.Adapter(SupportFragmentManager);
            ViewPager viewPager = FindViewById<ViewPager>(Resource.Id.vpTabs);
            viewPager.Adapter = fragmentAdapter;
            TabLayout tabLayout = FindViewById<TabLayout>(Resource.Id.tlTabs);
            tabLayout.SetupWithViewPager(viewPager);
        }
        #endregion

        #region Public methods
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }
        #endregion
    }
}