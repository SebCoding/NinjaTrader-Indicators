/*
=========   Economic news indicator that shows the economic news on your chart			=========
// https://futures.io/ninjatrader/40490-economic-news-indicator-ninjatrader-8-available-download.html
 Version 1.0.1	(21 Sep 2016):	- Initial Release
 Version 1.0.2	(22 Sep 2016):	- Removed date. Shows announcement time in raw format
 Version 1.0.3	(25 Sep 2016):	- Fixed layout bug when no news items are available
 Version 1.0.4	(08 Oct 2016):	- Added time offset for adjusting dates to local time
 Version 1.0.5	(11 Oct 2016):	- Added timezone offset to actual event time
 Version 1.0.6	(17 Nov 2016):	- Parsing source date and time using en-US culture. Provided right hand margin setting
 Version 1.0.7	(20 Nov 2016):	- Renamed right hand margin to left, which it actually is :-)
 Version 1.0.8	(07 Jan 2017):	- Removed grid lines settings from OnStateChange method so indicator respects chart settings
 Version 1.0.9	(31 Jan 2017):	- Added feature NewOnTop to reverse the order of news
 Version 1.1.02	(31 Jan 2017):	- Added feature Opacity for Historical News to dim the appearance of news events that have past
								- Added feature Minutes Upcoming News and Minutes to Expire to eliminate news items in a time window before and after events
								- Fixed a parsing bug to eliminated &lt; and &gt; xml entities from appearing in data
 Version "TT" 1.2  [re-coded by TazoTodua-gmail] (20 Nov 2017):
								- Cleaned code;  Re-sorted the inputs; Removed unnecessary comments From 1400 lines into 700 lines (without reducement of any functionality );  Hard-coded variables and codes transition into dynamic variables;
								- Added missing impact type: "Holiday" 
								- Added checkbox to ON/OFF country filter quickly (no longer need to modify symbols list)
								- Added option to hide any COLUMN from outputed table
								- Added option to individually hide TITLE and Header rows
								- Added option to set  start position (TopMargin) of output
								- Added option to show only TODAY's news
								- Added option to Hide news older than XYZ hours
								- Added option to AutoRefresh itself in every XYZ minutes
								- columns order(sequence) depends on the "NewsItem" model items order
								- corrected distance between columns and titles
								- added option to change PM/24 hrs format
								- added option to show day number
 Version "TT" 1.23  	(5 Dec 2017):	
								- bug fixes
								- Added auto-offset to user's timezone
								- Added option to show divisor between columns
								- Added option to show events in next XYZ minutes (not only 24 hours)
 Version 2 [modified by GitHub user SebCoding: seblife@pm.me} (13-sep-2024):
                                - Fixed bug: Indicator failing to display after max request to news server constantly being exceeded.
												
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Drawing;

using SharpDX.Direct2D1;
using SharpDX;
using SharpDX.DirectWrite;
using System.Net.Http;
using System.Collections.Specialized;
using System.Reflection;

using Indicators.awxEconomicNews_namespace2;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;

namespace Indicators.awxEconomicNews_namespace2
{
    #region NewsImpact Enum
    public enum NewsImpact      // Indicates the impact of a specific news item
    {
        Unknown = 0,
        Error = 1,
        Low = 2,
        Medium = 3,
        High = 4,
        Holiday = 5
    }
    #endregion

    #region NewsItemLayout Model Class
    public class NewsItemLayout
    {

        public NewsItem NewsItem { get; set; }  // Data source 
        public NewsImpact Impact { get; set; }  // Impact of the news item. We have it here so we can render the item in the correct color  
        public Dictionary<string, TextLayout> text_layouts = new Dictionary<string, TextLayout>();  //Text Layouts for rendering parts
    }
    #endregion

    #region NewsItem Model Class
    public class NewsItem       // Holds one specific news item
    {
        public string Country { get; set; }
        public DateTime Time { get; set; }
        public NewsImpact Impact { get; set; }
        public string Forecast { get; set; }
        public string Previous { get; set; }
        public string Title { get; set; }
        //public Dictionary<string,dynamic> els=new Dictionary<string,dynamic>();
    }
    #endregion
}


namespace NinjaTrader.NinjaScript.Indicators
{
    [CategoryDefaultExpanded(false)]
    [Gui.CategoryOrder("Ignore this Section (do not update fields)", 7000001)]
    public class awxEconomicNews2 : Indicator
	{
        #region Constants
        // URL where news would be retrieved from
        private const string NEWS_URL = "http://nfs.faireconomy.media/ff_calendar_thisweek.xml";
		// previously, http://cdn-nfs.faireconomy.media/ff_calendar_thisweek.xml, http://www.forexfactory.com/ffcal_week_this.xml		
		#endregion

		#region Public Accessors
        #region Items To Show 
        [NinjaScriptProperty]
		[Display(GroupName = "1. Items To Show",	Name = "Show Low Impact News",		Order = 1)]
		public bool _showLowImpactNews		{ get; set; }	
		 
		[NinjaScriptProperty]
		[Display(GroupName = "1. Items To Show",	Name = "Show Medium Impact News",	Order = 2)]
		public bool _showMediumImpactNews	{ get; set; }
 
		[NinjaScriptProperty]
		[Display(GroupName = "1. Items To Show",	Name = "Show High Impact News",		Order = 3)]
		public bool _showHighImpactNews		{ get; set; }

		[NinjaScriptProperty]
		[Display(GroupName = "1. Items To Show",	Name = "Show Holiday Impact News",	Order = 4)]
		public bool _showHolidayImpactNews	{ get; set; }

		[NinjaScriptProperty]
		[Display(GroupName = "1. Items To Show",	Name = "Show Unknown Impact News",	Order = 5)]
		public bool _showUnknownImpactNews	{ get; set; }

		[NinjaScriptProperty]
		[Display(GroupName = "1. Items To Show",	Name = "Show Errors",				Order = 6)]
		public bool _showErrors				{ get; set; } 
		#endregion
		
		#region Colors
		[XmlIgnore]
		[Display(GroupName = "2. Colors",	Name = "News Heading Color",		ResourceType = typeof(Custom.Resource),		Order = 1)]
		public System.Windows.Media.Brush _newsHeadingItemColor			{ get; set; }
			[Browsable(false)]
			public string _newsHeadingItemColorSerialize
			{
				get { return Serialize.BrushToString(_newsHeadingItemColor); }   			set { _newsHeadingItemColor = Serialize.StringToBrush(value); }
			}

		[XmlIgnore]
		[Display(GroupName = "2. Colors",	Name = "Low Impact News Color",		ResourceType = typeof(Custom.Resource),		Order = 2)]
		public System.Windows.Media.Brush _lowImpactNewsItemColor		{ get; set; }	
			[Browsable(false)]
			public string _lowImpactNewsItemColorSerialize
			{
				get { return Serialize.BrushToString(_lowImpactNewsItemColor); }   			set { _lowImpactNewsItemColor = Serialize.StringToBrush(value); }
			}
		
		[XmlIgnore]
		[Display(GroupName = "2. Colors",	Name = "Medium Impact News Color",	ResourceType = typeof(Custom.Resource),		Order = 3)]
		public System.Windows.Media.Brush _mediumImpactNewsItemColor	{ get; set; }	
			[Browsable(false)]
			public string _mediumImpactNewsItemColorSerialize
			{
				get { return Serialize.BrushToString(_mediumImpactNewsItemColor); }   		set { _mediumImpactNewsItemColor = Serialize.StringToBrush(value); }
			}

		[XmlIgnore]
		[Display(GroupName = "2. Colors",	Name = "High Impact News Color",	ResourceType = typeof(Custom.Resource),		Order = 4)]
		public System.Windows.Media.Brush _highImpactNewsItemColor		{ get; set; }
			[Browsable(false)]
			public string _highImpactNewsItemColorSerialize
			{
				get { return Serialize.BrushToString(_highImpactNewsItemColor); }			set { _highImpactNewsItemColor = Serialize.StringToBrush(value); }
			}
		
 		[XmlIgnore]
		[Display(GroupName = "2. Colors",	Name = "Unknown Impact News Color",	ResourceType = typeof(Custom.Resource),		Order = 5)]
		public System.Windows.Media.Brush _unknownImpactNewsItemColor	{ get; set; }	
			[Browsable(false)]
			public string _unknownImpactNewsItemColorSerialize
			{
				get { return Serialize.BrushToString(_unknownImpactNewsItemColor); }   		set { _unknownImpactNewsItemColor = Serialize.StringToBrush(value); }
			}

 		[XmlIgnore]
		[Display(GroupName = "2. Colors",	Name = "Unknown Impact News Color",	ResourceType = typeof(Custom.Resource),		Order = 5)]
		public System.Windows.Media.Brush _holidayImpactNewsItemColor	{ get; set; }	
			[Browsable(false)]
			public string _holidayImpactNewsItemColorSerialize
			{
				get { return Serialize.BrushToString(_holidayImpactNewsItemColor); }   		set { _holidayImpactNewsItemColor = Serialize.StringToBrush(value); }
			}

		[XmlIgnore]
		[Display(GroupName = "2. Colors",	Name = "Error Items Color",  		ResourceType = typeof(Custom.Resource),		Order = 6)]
		public System.Windows.Media.Brush _errorItemColor				{ get; set; }	
			[Browsable(false)]
			public string _errorItemColorSerialize
			{
				get { return Serialize.BrushToString(_errorItemColor); }					set { _errorItemColor = Serialize.StringToBrush(value); }
			}

		[Range(0, 100)] 
		[Display(GroupName = "2. Colors",	Name = "Opacity for historical news.",  		Order = 20)]
		public double areaOpacity				{ get; set; }
		#endregion

		#region Fonts and Aligments
		[NinjaScriptProperty]
		[Display(GroupName = "3a. Header",	Name = "Show top NEWS title",					Order = 0)]
		public bool _showBigTitle				{ get; set; }
		
		[NinjaScriptProperty]
		[Display(GroupName = "3a. Header",	Name = "Show Header row",						Order = 0)]
		public bool _showColumnsHeader			{ get; set; }
		
	    [NinjaScriptProperty]
	    [Display(GroupName = "3b. Font and Alignment",	Name = "News Heading Font",			Order = 1)]
	    public SimpleFont _newsHeadingFont		{ get; set; }	

	    [NinjaScriptProperty]
	    [Display(GroupName = "3b. Font and Alignment",	Name = "News Item Font",			Order = 2)]
	    public SimpleFont _newsItemFont			{ get; set; }	
		
	    [NinjaScriptProperty]
	    [Display(GroupName = "3b. Font and Alignment",	Name = "Left Hand Margin",			Order = 3)]
	    public int _leftHandMargin				{ get; set; }
		
	    [NinjaScriptProperty]
	    [Display(GroupName = "3b. Font and Alignment",	Name = "Top Margin",				Order = 4)]
	    public int _topMargin					{ get; set; }
		
	    [NinjaScriptProperty]
	    [Display(GroupName = "3b. Font and Alignment",	Name = "Space Between Columns",		Order = 4)]
	    public int SpaceBetweenColumns			{ get; set; } 
		
	    [NinjaScriptProperty]
	    [Display(GroupName = "3b. Font and Alignment",	Name = "Divider between columns",	Order = 4)]
	    public bool _showColumnDivider			{ get; set; } 
		
	    [NinjaScriptProperty]
	    [Display(GroupName = "3b. Font and Alignment",	Name = "Space Between Rows",		Order = 4)]
	    public int SpaceBetweenRows				{ get; set; } 
		
		[NinjaScriptProperty]
		[Display(GroupName = "3b. Font and Alignment",	Name = "Latest News On Top",		Order = 5)]
		public bool _NewItemsOnTop				{ get; set; }
		
		[NinjaScriptProperty]
		[Display(GroupName = "3b. Font and Alignment",	Name = "Table Top Title",			Order = 5)]
		public string TableBigTitle				{ get; set; }
		#endregion

		#region Times
		[NinjaScriptProperty]
		[Display(GroupName = "3c. Times",		Name = "Show day number",		Order = 5,	Description = "")]
		public bool _ShowDayNumber				{ get; set; }
		
		[NinjaScriptProperty]
		[Display(GroupName = "3c. Times",		Name = "Show In 24 hr format",	Order = 6,	Description = "otherwise AM/PM format")]
		public bool _ShowIn24hr_Format			{ get; set; }
		
		[NinjaScriptProperty]
		[Display(GroupName = "3c. Times",		Name = "Auto Timezone Offset",	Order = 21,	Description = "Automatically set your local timezone")]
		public bool _AutoTimezoneOffset			{ get; set; }
		
		[NinjaScriptProperty]
		[Display(GroupName = "3c. Times",		Name = "Manual TimeZone Offset",Order = 22,	Description = "The amount of hours the original time has to be offset")]
		public double _timeZoneOffset			{ get; set; }

		[NinjaScriptProperty]
        [Range(15, 1440)]
        [Display(GroupName = "3c. Times",		Name = "Auto-Refresh minutes",	Order = 51,	Description = "Automatically Data refresh in minutes")]
		public int DataRefreshInterval		{ get; set; }
		#endregion
		
		#region Filter
		[NinjaScriptProperty]
		[Display(GroupName = "8. Filters",		Name = "Enable Country Filter",	Order = 11)]
		public bool _enableCountryFilter		{ get; set; }

		[NinjaScriptProperty]
		[Display(GroupName = "8. Filters",		Name = "Country Filter",		Order = 12,	Description = "Comma seperated list of countries(currencies): USD,GBP,CAD... to be displayed")]
		public string _countryFilter			{ get; set; }
		
		[NinjaScriptProperty]
		[Display(GroupName = "8. Filters",		Name = "hide columns",			Order = 13,	Description = "Comma seperated list of columns oyu want to hide")]
		public string hide_these_columns		{ get; set; }

		[NinjaScriptProperty]
		[Display(GroupName = "8. Filters",		Name = "Minutes Upcoming News", Order = 4,	Description = "Minutes before news release to include in")]
		public int _timeUpcomingEventsInMinutes	{ get; set; } 
		
		[NinjaScriptProperty]
		[Display(GroupName = "8. Filters",		Name = "ShowOnlyToday",			Order = 7,	Description = "Show only current day's news?")]
		public bool _showOnlyTodayNews			{ get; set; }
		
		[NinjaScriptProperty]
		[Display(GroupName = "8. Filters",		Name = "Dont show older than",	Order = 6,	Description = "Dont show news older than i.e. 1440 minutes")]
		public int _dontShowOlderThan			{ get; set; }
		#endregion
		#endregion

		#region Private Members
		#region General
		private List<string> fields				= new List<string>();		// our all columns
		private List<string> DontShowColumns	= new List<string>();		// dont show columns
		private List<string> columns_to_show	= new List<string>();		// our finally shown columns
		private List<string> impactTypes		= new List<string>();		// our impact types	
        private float WidestCompleteItem 		= 0.0f;
		private float SpaceBetweenTitleAndLine	= 1;	 
        private Dictionary <string,float> widestTexts		= new Dictionary <string,float>{ };
		// Text format used to create the head & item formats
        private Dictionary <string,TextFormat> _textformats	= new Dictionary <string,TextFormat>{
            { "newsHeading",	null },
            { "newsItem",		null }
        };
		private int _arrayWalker		= 0;				// General purpose counter to walk through arrays
		// SharpDX layout object for heading & items
        private Dictionary <string,TextLayout> _textlayouts_SubHeading	= new Dictionary <string,TextLayout>{};
        private Dictionary <string,TextLayout> _textlayouts_Heading	= new Dictionary <string,TextLayout>{     { "BigTitle",	null }     };
        private Dictionary <string,float> ColumnWidths	= new Dictionary <string,float>{};
		private bool lastBarOnChart			= false;				// check last bar on chart
		private  bool _refreshIsProcessing	= false;				// Used to indicate if we are currently in the process of drawing the list of news items		//removed volatile
	    private  List<NewsItem> _newsItems	= new List<NewsItem>();	// Holds a list of all news items																//removed volatile
        private double chosenTimezoneOffset	=0;						// Will be set according to user-option
		private List<string> _filterList	= null;					// Holds the list of countries the user is interested in
		#endregion
		#region Text Rendering and Layout
		private float _newsItemTextHeight	= 0.0f;					// The height of a news item line
		private float RowPositionX	= 0.0f;	
		private float RowPositionY	= 0.0f;	
		//Show or not 
        private Dictionary <string,bool> _showTypes	=  new Dictionary <string,bool>();
        private Dictionary <string,System.Windows.Media.Brush> _impact_colors=  new Dictionary <string,System.Windows.Media.Brush>();
        private Dictionary <string,System.Windows.Media.Brush> _colors=  new Dictionary <string,System.Windows.Media.Brush>();
		
        private Dictionary <string,Vector2> startPositions		= new Dictionary <string,Vector2>();
		private List<NewsItemLayout> _itemLayouts = null;		// A list of all the text layouts for rendering news items
		private float _subHeadingXOffset	= 0.0f;				// The offset horizontal of the sub heading text
		private bool Allowed_To_Run			= true;				// for general use
		private bool needs_to_update		= false;            // for general use
        #endregion
        #endregion

        #region Public Properties that should be Private

        // We made these 2 properties public to benefit from persistence through serialization.
        // Making these properties hidden through [Browsable(false)] disables the serialization.
        // Unfortunately, these should be hidden but now appear in the UI because they are public.
        // We need to find another way to have these 2 properties private and still have persistence and serialization.

        [NinjaScriptProperty]
        [Display(GroupName = "Ignore this Section (do not update fields)", Name = "LastDataUpdateTime")]
        public DateTime LastDataUpdateTime { get; set; }

        [NinjaScriptProperty]
        [Display(GroupName = "Ignore this Section (do not update fields)", Name = "XMLNewsRawData")]
        public string XMLNewsRawData { get; set; }
        #endregion

        #region semi functions
        private bool NullOrZero(dynamic smth)				{ return (smth==null ||  smth.Count<=0); }
		private string Normalize_(string smth)				{ return (string.IsNullOrEmpty(smth) ? "" : smth); }
		private string Normalize_(XmlNode xmlnd, string key){ return (xmlnd.SelectSingleNode(key) == null ? "" : xmlnd.SelectSingleNode(key).InnerText); } 
		private string random_key(string[] strs)	{  return strs[new Random().Next(0,strs.Length) ]; }
		private TextLayout random_item(Dictionary <string,TextLayout> dict)	{ string[] strs=dict.Keys.ToArray();  return dict[strs[new Random().Next(0,strs.Length) ]]; }
		public List<string> ObjectPropertyNames(dynamic Obj)	{  List<string> x= new List<string>(); foreach(PropertyInfo each in Obj.GetType().GetProperties()) {  x.Add(each.Name);  }  return x; }
		private void EmptyColletion (System.Collections.IEnumerable collection) {
			foreach (object obj in collection) {
				IDisposable disp = obj as IDisposable;
				if (disp != null)	{	disp.Dispose();  }
			}
			collection = null;
		}
        #endregion

		#region State Change Overrides
		/// <summary>
		/// Called when the state of the indicator changes
		/// </summary>
		protected override void OnStateChange()
		{
            if (State == State.SetDefaults)
			{
				Description = @"Economic news indicator that shows the economic news on your chart";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				DisplayInDataBox = false;
				DrawOnPricePanel = true;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive = true;
				
				//=======user inputs=======//
				//
				_showColumnsHeader			= true;
				_showBigTitle				= true;
				//
				_showLowImpactNews			= true;			
				_showMediumImpactNews		= true;
				_showHighImpactNews			= true;
				_showHolidayImpactNews		= true;
				_showUnknownImpactNews		= true;
				_showErrors					= true;
				//
				_lowImpactNewsItemColor		= System.Windows.Media.Brushes.Chartreuse;
				_mediumImpactNewsItemColor	= System.Windows.Media.Brushes.Orange;	
				_highImpactNewsItemColor	= System.Windows.Media.Brushes.DeepPink;	
				_holidayImpactNewsItemColor	= System.Windows.Media.Brushes.Yellow;
				_unknownImpactNewsItemColor	= System.Windows.Media.Brushes.Silver;
				_errorItemColor				= System.Windows.Media.Brushes.Red;
				//
				_newsHeadingItemColor		= System.Windows.Media.Brushes.DodgerBlue;	
				_newsHeadingFont	= new SimpleFont("Consolas", 12) { Bold = true };
				_newsItemFont		= new SimpleFont("Consolas", 12);	
				_leftHandMargin		= 10;	
				areaOpacity			= 80;
				_topMargin			= 20;	
				SpaceBetweenColumns = 5;
				_showColumnDivider	= true;
				SpaceBetweenRows	= 0;
				_enableCountryFilter= false;
				_countryFilter		= "USD,GBP,CAD,EUR,JPY";		
				hide_these_columns	= "ColumnName1,...";
				DataRefreshInterval	= 60;
				_AutoTimezoneOffset	= true;
				_timeZoneOffset		= -5.0d;
				_ShowIn24hr_Format	= true;
				_ShowDayNumber		= true;
				_timeUpcomingEventsInMinutes = 1440;
				_dontShowOlderThan	= 480;
				_showOnlyTodayNews	= false;
				_NewItemsOnTop		= true;
				TableBigTitle		= "News Feed";
			}
			else if (State == State.Configure)
			{
                //Setup the initial state of our indicator
				chosenTimezoneOffset = _AutoTimezoneOffset ?  (TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow)).TotalHours  : _timeZoneOffset;
				_refreshIsProcessing = false;

				//Text format for our news heading and items
				_textformats["newsHeading"] = new TextFormat(new SharpDX.DirectWrite.Factory(),
												  _newsHeadingFont.Family.ToString(),
												  _newsHeadingFont.Bold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal,
												   _newsHeadingFont.Italic ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal,
												  (float)_newsHeadingFont.Size);

				_textformats["newsItem"] = new TextFormat(new SharpDX.DirectWrite.Factory(),
											   _newsItemFont.Family.ToString(),
											   _newsItemFont.Bold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal,
												_newsItemFont.Italic ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal,
											   (float)_newsItemFont.Size);
				
				//Split the filter to be useful later
				_filterList = _countryFilter.Split(',').ToList() ;
				DontShowColumns = hide_these_columns.Split(',').ToList();
				fields =  ObjectPropertyNames(new NewsItem());
				columns_to_show = fields.Except(DontShowColumns).ToList(); 
				impactTypes = Enum.GetNames(typeof(NewsImpact)).ToList();
				_colors["newsHeading"] = _newsHeadingItemColor;
				//
				_showTypes["Low"]		= _showLowImpactNews;			
				_showTypes["Medium"]	= _showMediumImpactNews;
				_showTypes["High"]		= _showHighImpactNews;
				_showTypes["Holiday"]	= _showHolidayImpactNews;
				_showTypes["Unknown"]	= _showUnknownImpactNews;
				_showTypes["Error"]		= _showErrors;
				//
				_impact_colors["Low"] 		= _lowImpactNewsItemColor;
				_impact_colors["Medium"]	= _mediumImpactNewsItemColor;	
				_impact_colors["High"]		= _highImpactNewsItemColor;	
				_impact_colors["Holiday"]	= _holidayImpactNewsItemColor;
				_impact_colors["Unknown"]	= _unknownImpactNewsItemColor;
				_impact_colors["Error"]		= _errorItemColor;
				//
				
			}
			else if (State == State.Terminated)
			{
				//General cleanup
				_newsItems = null;				//set null
				_filterList = null;				//set null
				_colors.Clear();				//Brush cleanup
				_impact_colors.Clear();			//Brush cleanup
				EmptyColletion(_textformats);				//Dispose Text formats
				EmptyColletion(_textlayouts_Heading);		//Dispose Text layouts
				EmptyColletion(_textlayouts_SubHeading);	//Dispose Text layouts
				
				if (_itemLayouts != null)
				{
					foreach (NewsItemLayout currentLayout in _itemLayouts)
					{
						EmptyColletion(currentLayout.text_layouts);	//Dispose 
						currentLayout.text_layouts=null;
					}
				}
			}
	    }

	    #endregion

		#region DisplayName Override
		public override string DisplayName 
		{
			// get { return $"{this.GetType().Name}(Refreshed {LastDataUpdateTime.ToString("dd-MMM-yyyy, HH:mm")})"; }
			get { return $"{Name}(Refreshed {LastDataUpdateTime.ToString("dd-MMM-yyyy, HH:mm")})"; }
		}
		#endregion

	    #region Bar Update Overrides
		/// <summary>
		/// Called on bar update
		/// </summary>
	    protected override void OnBarUpdate()
	    {
			if(!Allowed_To_Run) return;
			
			//draw only when last bar
			lastBarOnChart = (State != State.Historical) || (Calculate == Calculate.OnBarClose && Count - 2 <= CurrentBar) || (Calculate!= Calculate.OnBarClose && Count - 1 <= CurrentBar);
			if (!lastBarOnChart) return;
			
			RefreshNewsItems();
	    }
	    #endregion

	    

		private string getNews()
		{
            //Holds the raw XML returned from the news service
            string rawXML = null;
            WebResponse newsResponse = null;    //should be disposed later
            try
            {
                string error_title = "";            //check if error_happens somewhere

                //Build the web request and get RawXML
                WebRequest newsRequest = WebRequest.Create(NEWS_URL);
                newsRequest.Credentials = CredentialCache.DefaultCredentials;
                newsResponse = newsRequest.GetResponse();

                //Parse the response if we got one back
                if (newsResponse != null)
                {
                    if (((HttpWebResponse)newsResponse).StatusCode == HttpStatusCode.OK)
                    {
                        //Get the XML out of the response, like:
                        //	<?xml version="1.0" encoding="windows-1252"?>
                        //	<weeklyevents>
                        //	  <event>   <title>Retail Sales m/m</title><country>NZD</country><date><![CDATA[09-23-2017]]></date><time><![CDATA[2:00pm]]></time><impact><![CDATA[High]]></impact><forecast /><previous />	</event>
                        using (Stream responseStream = newsResponse.GetResponseStream())
                        {
                            using (StreamReader responseReader = new StreamReader(responseStream))
                            {
                                rawXML = responseReader.ReadToEnd();
                            }
                        }
                    }
                    else { error_title = "Could not retrieve news. HTTP error occurred."; }
                }
                else { error_title = "Could not read response for news request."; }

                //if error happened
                if (!string.IsNullOrEmpty(error_title))
                {
					//NewsItem errorNewsItem = new NewsItem() { Title = error_title, Country = "N/A", Impact = NewsImpact.Error };
					//_newsItems.Add(errorNewsItem);
					string err_mes = $"[{this.GetType().Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}], error_title";
					Print(err_mes);
                    Log(err_mes, LogLevel.Error);
                }
            }
            catch (Exception e)
            {
                string err = $"***[{this.GetType().Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}], [ERROR]: {e.ToString()}";
                //Just bubble up the exception to the top
                Print(err);
                Log(e.ToString(), LogLevel.Error);
                //NewsItem errorNewsItem = new NewsItem() { Country = "N/A", Impact = NewsImpact.Error, Title = err };
                //_newsItems.Add(errorNewsItem);
                //throw e;
            }
            finally
            {
                //We are done, make sure we don't refresh again today
                if(newsResponse != null)
					newsResponse.Dispose();
                LastDataUpdateTime = DateTime.Now;
            }
			return rawXML;
        }

	    /// <summary>
	    /// Refreshes the current list of news items
	    /// </summary>
	    private void RefreshNewsItems()
	    {
            string error_title = "";
            //Print("\nRefreshNewsItems:");
          

            //if something stuck in previous cycle
            if (_refreshIsProcessing)
                return;

            _newsItems = new List<NewsItem>();  //Start the process from zero
            _refreshIsProcessing = true;        //Make sure everybody knows we are refreshing the list

			// If it is time to refresh news from web
			DateTime nextUpdate = LastDataUpdateTime.AddMinutes(DataRefreshInterval);

            //Print($"LastUpdate: {LastDataUpdateTime.ToString()}\nNow: {DateTime.Now.ToString()}\nNextUpdate: {nextUpdate.ToString()}");


//			string xml = (XMLNewsRawData.IsNullOrEmpty()) ? "Xml[ ]" : $"Xml[{XMLNewsRawData}]";
//			Print(xml);
            if (XMLNewsRawData.IsNullOrEmpty() || (nextUpdate <= DateTime.Now))
			{
				//Print($"Before GetNews");

				string data = getNews();
				if (!data.IsNullOrEmpty())
					XMLNewsRawData = data.Replace('\n', ' ');

                //Print($"After GetNews");
			}
			//else
			//{
			//	Print("Skipped GetNews");
			//}

			if (!XMLNewsRawData.IsNullOrEmpty())
			{
				//Populate the news list for rendering (Get the XML into a useable format)
				XmlDocument rawXmlDoc = new XmlDocument();
				rawXmlDoc.LoadXml(XMLNewsRawData);
				XmlNodeList eventNodes = rawXmlDoc.SelectNodes(@"//event");

				//Parse all the news even nodes we got back
				if (!NullOrZero(eventNodes))
				{
					foreach (XmlNode currentEvent in eventNodes)
					{
						//Create a new item for our list and add it
						NewsItem newsItem = null;
						try
						{
							//Suck out all the data for this single event
							string date = Normalize_(currentEvent, "date");
							string time = Normalize_(currentEvent, "time");
							string impact = Normalize_(currentEvent, "impact");

							//We need to have a non-empty values
							if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(time))
							{
								//If date is parseable(valid)
								DateTime eventDateTime;
								if (DateTime.TryParseExact(string.Format("{0} {1}", date, time), "MM-dd-yyyy h:mmtt", CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out eventDateTime))
								{   //CultureInfo.GetCultureInfo("en-US")

									newsItem = new NewsItem()
									{
										Country = Normalize_(currentEvent, "country"),
										Title = Normalize_(currentEvent, "title"),
										Time = eventDateTime.AddHours(chosenTimezoneOffset),      // offset by chosen timezone 
										Forecast = Normalize_(currentEvent, "forecast").Replace("&lt;", "").Replace("&gt;", ""),
										Previous = Normalize_(currentEvent, "previous").Replace("&lt;", "").Replace("&gt;", ""),
										//What is the impact of this news item?
										Impact = impactTypes.Contains(impact) ? (NewsImpact)Enum.Parse(typeof(NewsImpact), impact, true) : NewsImpact.Unknown,
									};
								}
								//date was invalid. maybe something changed in website source
								else
								{
									newsItem = new NewsItem() { Title = "Invalid date", Country = "N/A", Impact = NewsImpact.Error };
								}
							}
						}
						catch (Exception e)
						{
							Print(string.Format("[" + this.GetType().Name + "][" + System.Reflection.MethodBase.GetCurrentMethod().Name + "]-[ERROR]-{0}", e.ToString()));
							Log(e.ToString(), LogLevel.Error);
							//Just bubble up the exception
							throw e;
						}
						if (newsItem != null)
						{
							_newsItems.Add(newsItem);
						}
					}
					if (NullOrZero(_newsItems)) { error_title = "Cant parse news from source."; }
				}
				//If there is nothing for today, tell the user
				else { error_title = "No news items currently"; }
			}
            //If there is nothing for today, tell the user
            else { error_title = "No news items currently"; }


            //if error happened
            if (!string.IsNullOrEmpty(error_title))
            {
                //NewsItem errorNewsItem = new NewsItem() { Title = error_title, Country = "N/A", Impact = NewsImpact.Error };
                //_newsItems.Add(errorNewsItem);
                string err_mes = $"[{this.GetType().Name}][{System.Reflection.MethodBase.GetCurrentMethod().Name}], error_title";
                Print(err_mes);
                Log(err_mes, LogLevel.Error);
            }
            
            _refreshIsProcessing = false;
        }


        #region Render Overrides
            /// <summary>
            /// Called for our indicator to redraw itself
            /// </summary>
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
	    {
		  try
		  {
			if(!Allowed_To_Run) return;
			//if we have to ignore this
			if (Bars == null  || chartControl == null   || Bars.Instrument == null   || !IsVisible   || IsInHitTest   )				return;
			//Only render the list if we are not refreshing it at the moment
			if (_refreshIsProcessing)  return; 
			
			//============= Big Heading Title ============= //
			RowPositionX=_leftHandMargin;
			RowPositionY=_topMargin;
			if(_showBigTitle){
				RowPositionY +=  SpaceBetweenRows;
				startPositions["BigTitle"]		= new Vector2( _leftHandMargin, RowPositionY);
				// Draw the news heading
				_textlayouts_Heading["BigTitle"]= new TextLayout(Core.Globals.DirectWriteFactory, TableBigTitle, _textformats["newsItem"],		ChartPanel.W, (float)_newsHeadingFont.Size); 
				RenderTarget.DrawTextLayout(startPositions["BigTitle"], _textlayouts_Heading["BigTitle"], _colors["newsHeading"].ToDxBrush(RenderTarget));
				//Horizontal line below the heading
				RowPositionY += random_item(_textlayouts_Heading).Metrics.Height + SpaceBetweenTitleAndLine;
				RenderTarget.DrawLine(
					new Vector2(startPositions["BigTitle"].X, 													RowPositionY),
					new Vector2(startPositions["BigTitle"].X + _textlayouts_Heading["BigTitle"].Metrics.Width,	RowPositionY),
					_colors["newsHeading"].ToDxBrush(RenderTarget)
				);
			}
			//======= Create layouts for all the items so that we can measure them for rendering =====//
			CreateAggregateLayoutsAndMeasure();

			//============= Sub headings ============= //
			RowPositionX = _leftHandMargin;
			if (_showColumnsHeader){
				RowPositionY +=  SpaceBetweenRows; 
				foreach (string field_name in columns_to_show){ 
					_textlayouts_SubHeading[field_name] = new TextLayout(Core.Globals.DirectWriteFactory, field_name.ToUpper(), _textformats["newsItem"], ChartPanel.W, (float)_newsItemFont.Size);
					RenderTarget.DrawTextLayout(new Vector2( RowPositionX, RowPositionY), _textlayouts_SubHeading[field_name], _colors["newsHeading"].ToDxBrush(RenderTarget));
					ColumnWidths[field_name]= Math.Max( _textlayouts_SubHeading[field_name].Metrics.Width,   (widestTexts.ContainsKey(field_name) ? widestTexts[field_name] : 0 ) ) ;
					RowPositionX += ColumnWidths[field_name] + SpaceBetweenColumns ; 
				}
				//Horizontal line below the sub headings
				RowPositionY += random_item(_textlayouts_SubHeading).Metrics.Height + SpaceBetweenTitleAndLine;
				RenderTarget.DrawLine(
					new Vector2( _leftHandMargin, 	RowPositionY),
					new Vector2( RowPositionX,		RowPositionY),
					_colors["newsHeading"].ToDxBrush(RenderTarget)
				);
			}

			//============= Rows ============= //
			RowPositionX = _leftHandMargin;
			//Now we can just step through our layouts list and render all of them
			foreach(NewsItemLayout currentLayout in _itemLayouts.AsEnumerable()  )
			{
				// skip items that
				if (
						( DateTime.Now < currentLayout.NewsItem.Time.Subtract(TimeSpan.FromMinutes(_timeUpcomingEventsInMinutes)) ) 	// too far in future. 
					||	( DateTime.Now > currentLayout.NewsItem.Time.Add(TimeSpan.FromMinutes(_dontShowOlderThan)) ) 					// too old
					||	( _showOnlyTodayNews && currentLayout.NewsItem.Time.Date != DateTime.Now.Date )									// not passed Today option
				)   continue;

				RowPositionX=_leftHandMargin;
				RowPositionY += SpaceBetweenRows;
				RenderNewsItem( currentLayout,    new Vector2(RowPositionX, RowPositionY), _impact_colors[currentLayout.Impact.ToString()] );
				RowPositionY = RowPositionY + random_item(currentLayout.text_layouts).Metrics.Height;
			} 
		  }
		  catch(Exception e)	{
			Print(string.Format("["+this.GetType().Name+"]["+System.Reflection.MethodBase.GetCurrentMethod().Name+"]-[ERROR]-{0}", e.ToString()));
			Log(e.ToString(),LogLevel.Error);
		  }
	    }
		#endregion

		/// <summary>
		/// Creates a model holding all the layouts for a sinle item line and measures the widest of each
		/// </summary>
		private void CreateAggregateLayoutsAndMeasure()
		{
			//Make sure we have a valid list
			if(_itemLayouts != null) 
				_itemLayouts.Clear();	
			else 	
				_itemLayouts = new List<NewsItemLayout>();
			//Create an aggregate layout for each item
			foreach(NewsItem currentItem in _newsItems)
			{
				//We will only create items for the news items that the user is interested in
				if(!_enableCountryFilter || _filterList.Contains(currentItem.Country))
				{
					if (_showTypes[currentItem.Impact.ToString()] ){
					
						NewsItemLayout newLayout = new NewsItemLayout();
						//set properties
						newLayout.NewsItem	= currentItem;
						newLayout.Impact	= currentItem.Impact;
						//set text layouts
						int i = 0;
						foreach(var field_name in columns_to_show)	{
							var value = currentItem.GetType().GetProperty(field_name).GetValue(currentItem, null);
                            var value_final =  ( field_name != "Time" ? value :  (   (!_ShowDayNumber ? "" : "(" +((dynamic)value).Day.ToString() +") ") +( _ShowIn24hr_Format ? (((dynamic)value).Hour).ToString()+":"+(((dynamic)value).Minute).ToString() : ((dynamic)value).ToShortTimeString() )  )  ).ToString() ;
							newLayout.text_layouts[field_name]= new TextLayout(Core.Globals.DirectWriteFactory, value_final, _textformats["newsItem"], ChartPanel.W, (float)_newsItemFont.Size);
							//Now that we have the layout, measure the widest for each of its items
							widestTexts[field_name]= Math.Max( ((dynamic) newLayout.text_layouts[field_name]).Metrics.Width,  (widestTexts.ContainsKey(field_name) ? widestTexts[field_name] : 0 ) );
						}

                        _itemLayouts.Add(newLayout); 
					}
				}
			}
			if (_NewItemsOnTop) _itemLayouts.Reverse<NewsItemLayout>();
		}

		private void RenderNewsItem(NewsItemLayout newsItemLayout, Vector2 position, System.Windows.Media.Brush itemBrush)
		{
			System.Windows.Media.Brush brush = itemBrush.Clone();
			if (newsItemLayout.NewsItem.Time < DateTime.Now) { brush.Opacity = areaOpacity / 100f; }
			brush.Freeze();
			foreach (string field_name in columns_to_show){
				if( _showColumnDivider ) {
					RenderTarget.DrawLine(new Vector2(RowPositionX- SpaceBetweenColumns/2, RowPositionY), new Vector2(RowPositionX- SpaceBetweenColumns/2, RowPositionY+  newsItemLayout.text_layouts[field_name].Metrics.Height)
						, System.Windows.Media.Brushes.White.ToDxBrush(RenderTarget));
				}
				RenderTarget.DrawTextLayout( new Vector2(RowPositionX, RowPositionY), newsItemLayout.text_layouts[field_name], brush.ToDxBrush(RenderTarget)); 
				RowPositionX = RowPositionX  +  ColumnWidths[field_name] + SpaceBetweenColumns;
			}
		}  

	}
} 

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private awxEconomicNews2[] cacheawxEconomicNews2;
		public awxEconomicNews2 awxEconomicNews2(bool _showLowImpactNews, bool _showMediumImpactNews, bool _showHighImpactNews, bool _showHolidayImpactNews, bool _showUnknownImpactNews, bool _showErrors, bool _showBigTitle, bool _showColumnsHeader, SimpleFont _newsHeadingFont, SimpleFont _newsItemFont, int _leftHandMargin, int _topMargin, int spaceBetweenColumns, bool _showColumnDivider, int spaceBetweenRows, bool _newItemsOnTop, string tableBigTitle, bool _showDayNumber, bool _showIn24hr_Format, bool _autoTimezoneOffset, double _timeZoneOffset, int dataRefreshInterval, bool _enableCountryFilter, string _countryFilter, string hide_these_columns, int _timeUpcomingEventsInMinutes, bool _showOnlyTodayNews, int _dontShowOlderThan, DateTime lastDataUpdateTime, string xMLNewsRawData)
		{
			return awxEconomicNews2(Input, _showLowImpactNews, _showMediumImpactNews, _showHighImpactNews, _showHolidayImpactNews, _showUnknownImpactNews, _showErrors, _showBigTitle, _showColumnsHeader, _newsHeadingFont, _newsItemFont, _leftHandMargin, _topMargin, spaceBetweenColumns, _showColumnDivider, spaceBetweenRows, _newItemsOnTop, tableBigTitle, _showDayNumber, _showIn24hr_Format, _autoTimezoneOffset, _timeZoneOffset, dataRefreshInterval, _enableCountryFilter, _countryFilter, hide_these_columns, _timeUpcomingEventsInMinutes, _showOnlyTodayNews, _dontShowOlderThan, lastDataUpdateTime, xMLNewsRawData);
		}

		public awxEconomicNews2 awxEconomicNews2(ISeries<double> input, bool _showLowImpactNews, bool _showMediumImpactNews, bool _showHighImpactNews, bool _showHolidayImpactNews, bool _showUnknownImpactNews, bool _showErrors, bool _showBigTitle, bool _showColumnsHeader, SimpleFont _newsHeadingFont, SimpleFont _newsItemFont, int _leftHandMargin, int _topMargin, int spaceBetweenColumns, bool _showColumnDivider, int spaceBetweenRows, bool _newItemsOnTop, string tableBigTitle, bool _showDayNumber, bool _showIn24hr_Format, bool _autoTimezoneOffset, double _timeZoneOffset, int dataRefreshInterval, bool _enableCountryFilter, string _countryFilter, string hide_these_columns, int _timeUpcomingEventsInMinutes, bool _showOnlyTodayNews, int _dontShowOlderThan, DateTime lastDataUpdateTime, string xMLNewsRawData)
		{
			if (cacheawxEconomicNews2 != null)
				for (int idx = 0; idx < cacheawxEconomicNews2.Length; idx++)
					if (cacheawxEconomicNews2[idx] != null && cacheawxEconomicNews2[idx]._showLowImpactNews == _showLowImpactNews && cacheawxEconomicNews2[idx]._showMediumImpactNews == _showMediumImpactNews && cacheawxEconomicNews2[idx]._showHighImpactNews == _showHighImpactNews && cacheawxEconomicNews2[idx]._showHolidayImpactNews == _showHolidayImpactNews && cacheawxEconomicNews2[idx]._showUnknownImpactNews == _showUnknownImpactNews && cacheawxEconomicNews2[idx]._showErrors == _showErrors && cacheawxEconomicNews2[idx]._showBigTitle == _showBigTitle && cacheawxEconomicNews2[idx]._showColumnsHeader == _showColumnsHeader && cacheawxEconomicNews2[idx]._newsHeadingFont == _newsHeadingFont && cacheawxEconomicNews2[idx]._newsItemFont == _newsItemFont && cacheawxEconomicNews2[idx]._leftHandMargin == _leftHandMargin && cacheawxEconomicNews2[idx]._topMargin == _topMargin && cacheawxEconomicNews2[idx].SpaceBetweenColumns == spaceBetweenColumns && cacheawxEconomicNews2[idx]._showColumnDivider == _showColumnDivider && cacheawxEconomicNews2[idx].SpaceBetweenRows == spaceBetweenRows && cacheawxEconomicNews2[idx]._NewItemsOnTop == _newItemsOnTop && cacheawxEconomicNews2[idx].TableBigTitle == tableBigTitle && cacheawxEconomicNews2[idx]._ShowDayNumber == _showDayNumber && cacheawxEconomicNews2[idx]._ShowIn24hr_Format == _showIn24hr_Format && cacheawxEconomicNews2[idx]._AutoTimezoneOffset == _autoTimezoneOffset && cacheawxEconomicNews2[idx]._timeZoneOffset == _timeZoneOffset && cacheawxEconomicNews2[idx].DataRefreshInterval == dataRefreshInterval && cacheawxEconomicNews2[idx]._enableCountryFilter == _enableCountryFilter && cacheawxEconomicNews2[idx]._countryFilter == _countryFilter && cacheawxEconomicNews2[idx].hide_these_columns == hide_these_columns && cacheawxEconomicNews2[idx]._timeUpcomingEventsInMinutes == _timeUpcomingEventsInMinutes && cacheawxEconomicNews2[idx]._showOnlyTodayNews == _showOnlyTodayNews && cacheawxEconomicNews2[idx]._dontShowOlderThan == _dontShowOlderThan && cacheawxEconomicNews2[idx].LastDataUpdateTime == lastDataUpdateTime && cacheawxEconomicNews2[idx].XMLNewsRawData == xMLNewsRawData && cacheawxEconomicNews2[idx].EqualsInput(input))
						return cacheawxEconomicNews2[idx];
			return CacheIndicator<awxEconomicNews2>(new awxEconomicNews2(){ _showLowImpactNews = _showLowImpactNews, _showMediumImpactNews = _showMediumImpactNews, _showHighImpactNews = _showHighImpactNews, _showHolidayImpactNews = _showHolidayImpactNews, _showUnknownImpactNews = _showUnknownImpactNews, _showErrors = _showErrors, _showBigTitle = _showBigTitle, _showColumnsHeader = _showColumnsHeader, _newsHeadingFont = _newsHeadingFont, _newsItemFont = _newsItemFont, _leftHandMargin = _leftHandMargin, _topMargin = _topMargin, SpaceBetweenColumns = spaceBetweenColumns, _showColumnDivider = _showColumnDivider, SpaceBetweenRows = spaceBetweenRows, _NewItemsOnTop = _newItemsOnTop, TableBigTitle = tableBigTitle, _ShowDayNumber = _showDayNumber, _ShowIn24hr_Format = _showIn24hr_Format, _AutoTimezoneOffset = _autoTimezoneOffset, _timeZoneOffset = _timeZoneOffset, DataRefreshInterval = dataRefreshInterval, _enableCountryFilter = _enableCountryFilter, _countryFilter = _countryFilter, hide_these_columns = hide_these_columns, _timeUpcomingEventsInMinutes = _timeUpcomingEventsInMinutes, _showOnlyTodayNews = _showOnlyTodayNews, _dontShowOlderThan = _dontShowOlderThan, LastDataUpdateTime = lastDataUpdateTime, XMLNewsRawData = xMLNewsRawData }, input, ref cacheawxEconomicNews2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.awxEconomicNews2 awxEconomicNews2(bool _showLowImpactNews, bool _showMediumImpactNews, bool _showHighImpactNews, bool _showHolidayImpactNews, bool _showUnknownImpactNews, bool _showErrors, bool _showBigTitle, bool _showColumnsHeader, SimpleFont _newsHeadingFont, SimpleFont _newsItemFont, int _leftHandMargin, int _topMargin, int spaceBetweenColumns, bool _showColumnDivider, int spaceBetweenRows, bool _newItemsOnTop, string tableBigTitle, bool _showDayNumber, bool _showIn24hr_Format, bool _autoTimezoneOffset, double _timeZoneOffset, int dataRefreshInterval, bool _enableCountryFilter, string _countryFilter, string hide_these_columns, int _timeUpcomingEventsInMinutes, bool _showOnlyTodayNews, int _dontShowOlderThan, DateTime lastDataUpdateTime, string xMLNewsRawData)
		{
			return indicator.awxEconomicNews2(Input, _showLowImpactNews, _showMediumImpactNews, _showHighImpactNews, _showHolidayImpactNews, _showUnknownImpactNews, _showErrors, _showBigTitle, _showColumnsHeader, _newsHeadingFont, _newsItemFont, _leftHandMargin, _topMargin, spaceBetweenColumns, _showColumnDivider, spaceBetweenRows, _newItemsOnTop, tableBigTitle, _showDayNumber, _showIn24hr_Format, _autoTimezoneOffset, _timeZoneOffset, dataRefreshInterval, _enableCountryFilter, _countryFilter, hide_these_columns, _timeUpcomingEventsInMinutes, _showOnlyTodayNews, _dontShowOlderThan, lastDataUpdateTime, xMLNewsRawData);
		}

		public Indicators.awxEconomicNews2 awxEconomicNews2(ISeries<double> input , bool _showLowImpactNews, bool _showMediumImpactNews, bool _showHighImpactNews, bool _showHolidayImpactNews, bool _showUnknownImpactNews, bool _showErrors, bool _showBigTitle, bool _showColumnsHeader, SimpleFont _newsHeadingFont, SimpleFont _newsItemFont, int _leftHandMargin, int _topMargin, int spaceBetweenColumns, bool _showColumnDivider, int spaceBetweenRows, bool _newItemsOnTop, string tableBigTitle, bool _showDayNumber, bool _showIn24hr_Format, bool _autoTimezoneOffset, double _timeZoneOffset, int dataRefreshInterval, bool _enableCountryFilter, string _countryFilter, string hide_these_columns, int _timeUpcomingEventsInMinutes, bool _showOnlyTodayNews, int _dontShowOlderThan, DateTime lastDataUpdateTime, string xMLNewsRawData)
		{
			return indicator.awxEconomicNews2(input, _showLowImpactNews, _showMediumImpactNews, _showHighImpactNews, _showHolidayImpactNews, _showUnknownImpactNews, _showErrors, _showBigTitle, _showColumnsHeader, _newsHeadingFont, _newsItemFont, _leftHandMargin, _topMargin, spaceBetweenColumns, _showColumnDivider, spaceBetweenRows, _newItemsOnTop, tableBigTitle, _showDayNumber, _showIn24hr_Format, _autoTimezoneOffset, _timeZoneOffset, dataRefreshInterval, _enableCountryFilter, _countryFilter, hide_these_columns, _timeUpcomingEventsInMinutes, _showOnlyTodayNews, _dontShowOlderThan, lastDataUpdateTime, xMLNewsRawData);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.awxEconomicNews2 awxEconomicNews2(bool _showLowImpactNews, bool _showMediumImpactNews, bool _showHighImpactNews, bool _showHolidayImpactNews, bool _showUnknownImpactNews, bool _showErrors, bool _showBigTitle, bool _showColumnsHeader, SimpleFont _newsHeadingFont, SimpleFont _newsItemFont, int _leftHandMargin, int _topMargin, int spaceBetweenColumns, bool _showColumnDivider, int spaceBetweenRows, bool _newItemsOnTop, string tableBigTitle, bool _showDayNumber, bool _showIn24hr_Format, bool _autoTimezoneOffset, double _timeZoneOffset, int dataRefreshInterval, bool _enableCountryFilter, string _countryFilter, string hide_these_columns, int _timeUpcomingEventsInMinutes, bool _showOnlyTodayNews, int _dontShowOlderThan, DateTime lastDataUpdateTime, string xMLNewsRawData)
		{
			return indicator.awxEconomicNews2(Input, _showLowImpactNews, _showMediumImpactNews, _showHighImpactNews, _showHolidayImpactNews, _showUnknownImpactNews, _showErrors, _showBigTitle, _showColumnsHeader, _newsHeadingFont, _newsItemFont, _leftHandMargin, _topMargin, spaceBetweenColumns, _showColumnDivider, spaceBetweenRows, _newItemsOnTop, tableBigTitle, _showDayNumber, _showIn24hr_Format, _autoTimezoneOffset, _timeZoneOffset, dataRefreshInterval, _enableCountryFilter, _countryFilter, hide_these_columns, _timeUpcomingEventsInMinutes, _showOnlyTodayNews, _dontShowOlderThan, lastDataUpdateTime, xMLNewsRawData);
		}

		public Indicators.awxEconomicNews2 awxEconomicNews2(ISeries<double> input , bool _showLowImpactNews, bool _showMediumImpactNews, bool _showHighImpactNews, bool _showHolidayImpactNews, bool _showUnknownImpactNews, bool _showErrors, bool _showBigTitle, bool _showColumnsHeader, SimpleFont _newsHeadingFont, SimpleFont _newsItemFont, int _leftHandMargin, int _topMargin, int spaceBetweenColumns, bool _showColumnDivider, int spaceBetweenRows, bool _newItemsOnTop, string tableBigTitle, bool _showDayNumber, bool _showIn24hr_Format, bool _autoTimezoneOffset, double _timeZoneOffset, int dataRefreshInterval, bool _enableCountryFilter, string _countryFilter, string hide_these_columns, int _timeUpcomingEventsInMinutes, bool _showOnlyTodayNews, int _dontShowOlderThan, DateTime lastDataUpdateTime, string xMLNewsRawData)
		{
			return indicator.awxEconomicNews2(input, _showLowImpactNews, _showMediumImpactNews, _showHighImpactNews, _showHolidayImpactNews, _showUnknownImpactNews, _showErrors, _showBigTitle, _showColumnsHeader, _newsHeadingFont, _newsItemFont, _leftHandMargin, _topMargin, spaceBetweenColumns, _showColumnDivider, spaceBetweenRows, _newItemsOnTop, tableBigTitle, _showDayNumber, _showIn24hr_Format, _autoTimezoneOffset, _timeZoneOffset, dataRefreshInterval, _enableCountryFilter, _countryFilter, hide_these_columns, _timeUpcomingEventsInMinutes, _showOnlyTodayNews, _dontShowOlderThan, lastDataUpdateTime, xMLNewsRawData);
		}
	}
}

#endregion
