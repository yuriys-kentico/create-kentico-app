using System;
using System.Runtime.Serialization;

using App.Boilerplate.Core.Widgets;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(UserWidgetInfo), UserWidgetInfo.OBJECT_TYPE)]

namespace App.Boilerplate.Core.Widgets
{
    [Serializable]
    public class UserWidgetInfo : AbstractInfo<UserWidgetInfo>
    {
        public const string OBJECT_TYPE = "app_boilerplate.userwidget";

        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(
            typeof(UserWidgetInfoProvider),
            OBJECT_TYPE,
            "App_Boilerplate.UserWidget",
            nameof(UserWidgetInfoID),
            nameof(UserWidgetInfoLastModified),
            nameof(UserWidgetInfoGuid),
            nameof(UserWidgetCodeName),
            nameof(UserWidgetName),
            null, null, null, null)
        {
            ModuleName = "App.Boilerplate",
            TouchCacheDependencies = true,
        };

        [DatabaseField]
        public virtual int UserWidgetInfoID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(UserWidgetInfoID)), 0);
            set => SetValue(nameof(UserWidgetInfoID), value);
        }

        [DatabaseField]
        public virtual string UserWidgetName
        {
            get => ValidationHelper.GetString(GetValue(nameof(UserWidgetName)), String.Empty);
            set => SetValue(nameof(UserWidgetName), value, String.Empty);
        }

        [DatabaseField]
        public virtual string UserWidgetCodeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(UserWidgetCodeName)), String.Empty);
            set => SetValue(nameof(UserWidgetCodeName), value);
        }

        [DatabaseField]
        public virtual string UserWidgetDescription
        {
            get => ValidationHelper.GetString(GetValue(nameof(UserWidgetDescription)), String.Empty);
            set => SetValue(nameof(UserWidgetDescription), value, String.Empty);
        }

        [DatabaseField]
        public virtual string UserWidgetIcon
        {
            get => ValidationHelper.GetString(GetValue(nameof(UserWidgetIcon)), String.Empty);
            set => SetValue(nameof(UserWidgetIcon), value, String.Empty);
        }

        [DatabaseField]
        public virtual string UserWidgetProperties
        {
            get => ValidationHelper.GetString(GetValue(nameof(UserWidgetProperties)), string.Empty);
            set => SetValue(nameof(UserWidgetProperties), value, String.Empty);
        }

        [DatabaseField]
        public virtual string UserWidgetView
        {
            get => ValidationHelper.GetString(GetValue(nameof(UserWidgetView)), String.Empty);
            set => SetValue(nameof(UserWidgetView), value);
        }

        [DatabaseField]
        public virtual Guid UserWidgetInfoGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(UserWidgetInfoGuid)), Guid.Empty);
            set => SetValue(nameof(UserWidgetInfoGuid), value);
        }

        [DatabaseField]
        public virtual DateTime UserWidgetInfoLastModified
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(UserWidgetInfoLastModified)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(UserWidgetInfoLastModified), value);
        }

        protected override void DeleteObject() => UserWidgetInfoProvider.DeleteUserWidgetInfo(this);

        protected override void SetObject() => UserWidgetInfoProvider.SetUserWidgetInfo(this);

        protected UserWidgetInfo(SerializationInfo info, StreamingContext context) : base(info, context, TYPEINFO)
        {
        }

        public UserWidgetInfo() : base(TYPEINFO)
        {
        }
    }
}