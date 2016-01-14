<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SessionBlockPreviewControl.ascx.cs" Inherits="BVNetwork.Attend.Views.Blocks.SessionBlockPreviewControl" %>
<%@ Register Namespace="EPiServer.Web.WebControls" TagPrefix="EPiServer" %>
<%@ Register Namespace="EPiServer.XForms.WebControls" TagPrefix="XForms" %>


<div class="darkarea">
    <div class="content">
        <div class="left">
            <a target="_top" class="" href='<%=EventUrl %>'>
                <img src="/Modules/BVNetwork.Attend/static/attend_blue.png" /></a>
        </div>
        <div class="left">
            <h1>
                <EPiServer:Translate runat="server" text="/attend/edit/attend" />
                <br />
                <%=(CurrentBlock as EPiServer.Core.IContent).Name %></h1>
            <EPiServer:Translate runat="server" text="/attend/edit/sessionat" />
            <EPiServer:property runat="server" propertyname="pageName" pagelinkproperty="EventPageBase"></EPiServer:property><br />
        </div>
        <div class="right">
            <a target="_top" class="btn btn-default" href='<%=EventUrl %>'>
                <EPiServer:Translate runat="server" text="/attend/edit/returntoevent" />
            </a>
        </div>
    </div>
</div>


<div class="content">
    <h2>
        <EPiServer:Translate runat="server" text="/attend/edit/editsessiondetails" />
    </h2>
    <div class="row">
    <div class="col-lg-6">
    <table class="table-responsive table table-hover">
        <tr>
            <td>
                <EPiServer:Translate runat="server" text="/attend/edit/start" />
            </td>
            <td>
                <EPiServer:property runat="server" propertyname="Start" id="PropertyEmail"></EPiServer:property></td>
        </tr>
        <tr>
            <td>
                <EPiServer:Translate runat="server" text="/attend/edit/end" />
            </td>
            <td>
                <EPiServer:property runat="server" propertyname="End" id="Code"></EPiServer:property></td>
        </tr>

        <tr>
            <td>
                <EPiServer:Translate runat="server" text="/attend/edit/numberofseats" />
            </td>
            <td>
                <EPiServer:property runat="server" propertyname="NumberOfSeats" id="Property2"></EPiServer:property></td>
        </tr>

        <tr>
            <td>
                <EPiServer:Translate runat="server" text="/attend/edit/mandatory" />
            </td>
            <td>
                <EPiServer:property runat="server" propertyname="Mandatory" id="Property1"></EPiServer:property></td>
        </tr>

        <tr>
            <td>
                <EPiServer:Translate runat="server" text="/attend/edit/introcontent" />
            </td>
            <td>
                <EPiServer:property runat="server" propertyname="IntroContent" id="Property3"></EPiServer:property></td>
        </tr>
                <tr>
            <td valign="top">
                <EPiServer:Translate runat="server" text="/attend/edit/trackid" />
            </td>
            <td>
                <EPiServer:property runat="server" propertyname="TrackID" id="Property4"></EPiServer:property>
                
            </td>
        </tr>
        <tr><td colspan="2"><EPiServer:Translate runat="server" text="/attend/edit/trackiddescription" /></td></tr>
    </table>
        </div></div>
</div>
<br />
<br />
<div class="well">
    <div class="content">
        <h2>
            <EPiServer:Translate runat="server" text="/attend/edit/participants" />
        </h2>
        <div class="thinborder">
            <asp:PlaceHolder runat="server" ID="NoParticipants">
                <EPiServer:Translate runat="server" text="/attend/edit/noparticipants" />
            </asp:PlaceHolder>

            <table cellpadding="5" cellspacing="" class="table table-responsive table-hover">
                <EPiServer:Property runat="server" id="ParticipantsContentArea">
                    <rendersettings tag="ListView" />
                </EPiServer:Property>
            </table>
        </div>
        <br />
    </div>
</div>
<div class="darkarea">
    <div class="content">
        <a target="_top" class="btn btn-default" href='<%=EventUrl %>'>
            <EPiServer:Translate runat="server" text="/attend/edit/returntoevent" />
        </a>
    </div>
</div>
