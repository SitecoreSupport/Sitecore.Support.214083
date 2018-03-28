namespace Sitecore.Support.Shell.Applications.ContentEditor.Pipelines.RenderContentEditorHeader
{

  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Resources;
  using Sitecore.Text;
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web.UI;
  using System.Web.UI.WebControls;
  using Sitecore.Analytics.Data;
  using Sitecore.Analytics.Pipelines.GetItemPersonalizationVisibility;
  using Sitecore.Globalization;
  using Sitecore.Pipelines;
  using Sitecore.Web.UI;
  using System.IO;
  using Telerik.Web.UI;
  
  public class ProfileCardsControl: System.Web.UI.WebControls.WebControl
  {
    public Item Item
    {
      get;
      set;
    }

    protected override void OnInit(EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");
      this.Page.Header.Controls.Add(new LiteralControl("\r\n    <script type='text/JavaScript' src='/sitecore/shell/Applications/Analytics/Personalization/Carousel/jquery.jcarousel.min.js'></script>\r\n    <link href='/sitecore/shell/Applications/Analytics/Personalization/Carousel/skin.css' rel='stylesheet' />\r\n    <script type='text/JavaScript' src='/sitecore/shell/Applications/Analytics/Personalization/Tooltip.js'></script>"));
      HtmlTextWriter htmlTextWriter = new HtmlTextWriter(new StringWriter());
      this.RenderProfileCards(this.Item, htmlTextWriter);

      RadToolTipManager radToolTipManager = new RadToolTipManager
      {
        Skin = "Metro",
        ID = "ToolTipManager" + ((this.Item != null) ? this.Item.ID.ToShortID().ToString() : string.Empty),
        CssClass = "scRadTooltipManager"
      };

      this.Controls.Add(radToolTipManager);
      ProfileCardsControl.InitializeTooltip(radToolTipManager);
      this.Controls.Add(new LiteralControl(htmlTextWriter.InnerWriter.ToString()));
      base.OnLoad(e);
    }

    private void RenderProfileCards(Item item, HtmlTextWriter output)
    {
      if (item == null)
      {
        return;
      }
      if (!this.RenderPersonalizationPanel(item))
      {
        return;
      }
      Assert.ArgumentNotNull(output, "output");
      string text = Translate.Text("Edit the profile cards associated with this item.");
      ImageBuilder imageBuilder = new ImageBuilder();
      UrlString urlString = new UrlString(Images.GetThemedImageSource("Office/32x32/photo_portrait.png", ImageDimension.id32x32));
      imageBuilder.Src = urlString.ToString();
      imageBuilder.Class = "scEditorHeaderCustomizeProfilesIcon";
      imageBuilder.Alt = text;
      if (!item.Appearance.ReadOnly && item.Access.CanWrite())
      {
        output.Write("<a href=\"#\" class=\"scEditorHeaderCustomizeProfilesIcon\" onclick=\"javascript:return scForm.invoke('item:personalize')\" title=\"" + text + "\">");
        output.Write(imageBuilder.ToString());
        output.Write("</a>");
      }
      StringBuilder stringBuilder = new StringBuilder();
      HtmlTextWriter htmlTextWriter = new HtmlTextWriter(new StringWriter(stringBuilder));
      bool flag;
      this.RenderProfileCardIcons(item, htmlTextWriter, out flag);
      htmlTextWriter.Flush();
      if (flag)
      {
        if (!UIUtil.IsIE())
        {
          output.Write("<span class=\"scEditorHeaderProfileCards\">");
        }
        output.Write(stringBuilder.ToString());
        if (!UIUtil.IsIE())
        {
          output.Write("</span>");
        }
      }
    }

    private bool RenderPersonalizationPanel(Item item)
    {
      if (CorePipelineFactory.GetPipeline("getItemPersonalizationVisibility", string.Empty) == null)
      {
        return true;
      }
      GetItemPersonalizationVisibilityArgs getItemPersonalizationVisibilityArgs = new GetItemPersonalizationVisibilityArgs(true, item);
      CorePipeline.Run("getItemPersonalizationVisibility", getItemPersonalizationVisibilityArgs);
      return getItemPersonalizationVisibilityArgs.Visible;
    }

    private void RenderProfileCardIcons(Item item, HtmlTextWriter output, out bool hasCardsConfigured)
    {
      hasCardsConfigured = false;
      Assert.ArgumentNotNull(output, "output");
      if (item == null)
      {
        return;
      }
      TrackingField trackingField;
      IEnumerable<ContentProfile> profiles = ProfileUtil.GetProfiles(item, out trackingField);
      if (trackingField == null)
      {
        return;
      }
      int num = 0;
      foreach (ContentProfile current in profiles)
      {
        if (current != null)
        {
          Item profileItem = current.GetProfileItem();
          if (profileItem != null)
          {
            if (current.Presets == null || current.Presets.Count == 0)
            {
              if (ProfileUtil.HasPresetData(profileItem, trackingField))
              {
                this.RenderEditorHeaderSeparator(output, (num == 0) ? "scEditorHeaderSeperatorFirstLine" : string.Empty);
                this.RenderProfileCardIcon(item, profileItem, null, output);
                num++;
              }
            }
            else
            {
              int num2 = 0;
              foreach (KeyValuePair<string, float> current2 in current.Presets)
              {
                Item presetItem = current.GetPresetItem(current2.Key);
                if (presetItem != null)
                {
                  if (num2 == 0)
                  {
                    this.RenderEditorHeaderSeparator(output, (num == 0) ? "scEditorHeaderSeperatorFirstLine" : string.Empty);
                  }
                  this.RenderProfileCardIcon(item, profileItem, presetItem, output);
                  num++;
                  num2++;
                }
              }
            }
          }
        }
      }
      hasCardsConfigured = (num > 0);
    }

    private void RenderEditorHeaderSeparator(HtmlTextWriter output, string customClassName)
    {
      Assert.ArgumentNotNull(output, "output");
      Assert.ArgumentNotNull(customClassName, "customClassName");
      string arg = string.Empty;
      if (!string.IsNullOrEmpty(customClassName))
      {
        arg = " " + customClassName;
      }
      output.Write("<div class=\"scEditorHeaderSeperator\"><span class=\"scEditorHeaderSeperatorLine{0}\"></span></div>", arg);
    }

    private void RenderProfileCardIcon(Item contextItem, Item profileItem, Item presetItem, HtmlTextWriter output)
    {
      Assert.ArgumentNotNull(contextItem, "contextItem");
      Assert.ArgumentNotNull(profileItem, "profileItem");
      Assert.ArgumentNotNull(output, "output");
      string url = (presetItem != null) ? ProfileUtil.UI.GetPresetThumbnail(presetItem) : ProfileUtil.UI.GetProfileThumbnail(profileItem);
      ImageBuilder imageBuilder = new ImageBuilder();
      UrlString urlString = new UrlString(url);
      imageBuilder.Src = urlString.ToString();
      imageBuilder.Class = "scEditorHeaderProfileCardIcon";
      string uniqueID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("profileIcon");
      string text = (presetItem == null) ? (profileItem.ID.ToShortID() + "|" + profileItem.Language) : (presetItem.ID.ToShortID() + "|" + presetItem.Language);
      if (contextItem.Appearance.ReadOnly || !contextItem.Access.CanWrite())
      {
        output.Write("<a id=\"{1}\" href=\"#\" class=\"scEditorHeaderProfileCardIcon\" style=\"background-image:url('{0}'); background-repeat:no-repeat; background-position:center;\" onmouseover=\"showToolTipWithTimeout('{1}', '{2}', null, 500);\" onmouseout=\"cancelRadTooltip();\">", urlString, uniqueID, text);
        output.Write("</a>");
        return;
      }
      string personalizeProfileCommand = ProfileUtil.UI.GetPersonalizeProfileCommand(contextItem, profileItem);
      output.Write("<a id=\"{2}\" href=\"#\" class=\"scEditorHeaderProfileCardIcon\" onclick=\"javascript:return scForm.invoke('{1}')\" style=\"background-image:url('{0}'); background-repeat:no-repeat; background-position:center;\" onmouseover=\"showToolTipWithTimeout('{2}', '{3}', null, 500);\" onmouseout=\"cancelRadTooltip();\">", new object[]
      {
                urlString,
                personalizeProfileCommand,
                uniqueID,
                text
      });
      output.Write("</a>");
    }

    private static void InitializeTooltip(RadToolTipManager tooltip)
    {
      Assert.ArgumentNotNull(tooltip, "tooltip");
      tooltip.AnimationDuration = 200;
      tooltip.EnableShadow = true;
      tooltip.HideDelay = 1000;
      tooltip.Width = new Unit(340);
      tooltip.Height = new Unit(214);
      tooltip.ContentScrolling = ToolTipScrolling.None;
      tooltip.RelativeTo = ToolTipRelativeDisplay.Element;
      tooltip.Animation = ToolTipAnimation.Slide;
      tooltip.Position = ToolTipPosition.BottomCenter;
      tooltip.Skin = "Telerik";
      tooltip.MouseTrailing = false;
      tooltip.HideEvent = ToolTipHideEvent.LeaveTargetAndToolTip;
      tooltip.WebServiceSettings.Path = "/sitecore/shell/Applications/Analytics/Personalization/ToolTip/RenderToolTipService.asmx";
      tooltip.WebServiceSettings.Method = "OnTooltipUpdate";
      tooltip.ShowEvent = ToolTipShowEvent.FromCode;
      tooltip.OffsetY = 0;
      tooltip.AutoCloseDelay = 10000;
      tooltip.ShowCallout = true;
    }
  }
}