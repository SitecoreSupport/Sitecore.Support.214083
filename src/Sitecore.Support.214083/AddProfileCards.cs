namespace Sitecore.Support.Shell.Applications.ContentEditor.Pipelines.RenderContentEditorHeader
{
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditorHeader;
  public class AddProfileCards : Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditorHeader.AddProfileCards
    {
      public void Process(RenderContentEditorHeaderArgs args)
      {
        Assert.ArgumentNotNull(args, "args");
        Item item = args.Item;
      ProfileCardsControl control = new ProfileCardsControl
        {
          ID = "ProfileCardsControl" + args.Item.ID.ToShortID(),
          Item = item
        };


        Context.ClientPage.AddControl(args.Parent, control);

      }
    }
  
}