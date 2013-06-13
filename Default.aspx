<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="yumi._Default" %>


<%@ Register assembly="GMaps" namespace="Subgurim.Controles" tagprefix="cc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml"> 
<head>
    <title>Yumi</title>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8"/>
	<script src="http://maps.google.com/maps?file=api&v=2"type="text/javascript"> 
	</script>
    <style type="text/css">
        .style1
        {
            font-family: "Comic Sans MS";
            font-size: 50pt;
            text-align: center;
            color: DodgerBlue;           
        }
        #Select1
        {
            height: 24px;
            width: 200px;
            margin-bottom: 0px;
        }
        .style2
        {
            width: 131px;
        }
        .style3
        {
            width: 280px;
        }
        #form1
        {
            height: 729px;
        }
        #Img
        {
            text-align: center;
        }
        .tableStyle1
        {
            border: medium solid #000000;
            text-align: left;
        }
        .style4
        {
            width: 682px;
        }
        .style5
        {
            width: 461px;
        }
        .style7
        {
            width: 212px;
        }
     </style>
    
    <script type="text/javascript">
    
        function cleanUpForm() {
            var checkBoxes = document.getElementById('CheckBoxList1');
            var img = document.getElementById('divImg');
            checkBoxes.style.display = 'none';
            img.style.display = 'block';
        }
        window.onload = function () {
            var img = document.getElementById('divImg');
            img.style.display = 'none';
        }
       
        function getDropdownListSelectedText() {
            var DropdownList = document.getElementById('<%=DropDownList1.ClientID %>');
            //var SelectedIndex = DropdownList.selectedIndex;
            //document.getElementById('hello').innerHTML = SelectedIndex;
            var SelectedText = DropdownList.options[DropdownList.selectedIndex].text;
            if (SelectedText == "Location") {
                //document.getElementById('hello').innerHTML = SelectedIndex + "is selected";
                document.getElementById('novel').style.display = 'inline';
                document.getElementById('radiusTable').style.display = 'inline';
            }

        }
        
    </script>
</head>



<body onunload="GUnload()" style="height: 378px; width: 1306px;">

<div id="map" style="position: absolute; display: none; top:39px; left:11px; width:334px; height:226px; 
 -moz-outline-radius:20px; -moz-box-sizing:padding-box; -moz-outline-style:solid;-moz-outline-color:#9FB6CD; 
-moz-outline-width:10px;" >Map...</div>
    <form id="form1" runat="server">
    <table bgcolor="gray" align="center" 
        
        
        style="width: 940px; height: 207px; position: relative; top: -6px; left: -13px;"> 
    <tr><td align="center" class="style4">    
       <div class="style1">    
           Yumi
       </div>
    
       <table align="center" style="width: 900px; margin-left: 23px">
       <tr>
            <td>&nbsp;</td>
            <td class="style7"><b><font color="white">Neighborhood:</font></b></td>
            <td><b><font color="white">Cuisine:</font></b></td>
            <td><b><font color="white">Keyword:</font></b></td>
            <td><b><font color="white">Price:</font></b></td>
            <td class="style2">
                <asp:Button ID="Button2" runat="server" onclick="Button2_Click" Text="Test" 
                    Width="50px" />
            </td>
       </tr>
       <tr>
            <td>
                <asp:LinkButton ID="LinkButton1" runat="server" OnClick="LinkButton1_Click">settings</asp:LinkButton>&nbsp;
            </td>
            <td class="style7">
                <asp:DropDownList ID="DropDownList1" runat="server" height="20px" 
                Width="148px" onchange="getDropdownListSelectedText();" 
                    onselectedindexchanged="DropDownList1_SelectedIndexChanged">
                    <asp:ListItem>Location</asp:ListItem>
                </asp:DropDownList>
                &nbsp;
            </td>
            <td class="style3">
                &nbsp;<asp:DropDownList ID="DropDownList3" runat="server" Width="132px" 
                    height="22px">
                </asp:DropDownList>
            &nbsp;</td>
            <td class="style3">                                            
                <asp:TextBox ID="TextBox1" runat="server" Width="125px" Height="18px" 
                    ontextchanged="TextBox1_TextChanged"></asp:TextBox>&nbsp;                
            </td>
            <td class = "style3">
                <asp:DropDownList ID="DropDownList2" runat="server" height="24px" Width="75px" 
                    onselectedindexchanged="DropDownList2_SelectedIndexChanged">
                </asp:DropDownList>&nbsp
            </td>
            <td class="style2">
                <asp:Button ID="Button1" runat="server" Text="Submit" Width="94px" 
                Font-Bold="True" Font-Size="Medium" Height="28px" 
                onclick="Button1_Click" 
                    OnClientClick="cleanUpForm();" style="margin-top: 0px"/>&nbsp
            </td>
            </tr>
       </table>
    </td></tr>                   
    </table>        
    <table align="center" 
        
        style="width: 617px; height: 54px; position: relative; top: 0px; left: 260px;">
    <tr><td align="center">
        <asp:CheckBoxList ID="CheckBoxList1" runat="server" style="display:none"
               width="648px" Height="20px" Font-Bold="True" BorderWidth="1px" BorderColor="Gray"
               Font-Size="Medium" RepeatColumns="5" RepeatDirection="Horizontal">
        </asp:CheckBoxList>
        <table id="radiusTable" style="display:none;">
        <tr>
            <td>
            <label id="lat">Lat and Long of the location are</label>
            </td>
            <td>
            <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td>
                <label id="radius">Distance willing to travel</label>
            </td>
            <td>
                <asp:TextBox ID="TextBox3" runat="server"></asp:TextBox>
                
            </td>
        </tr>
        <tr>
            <td colspan=2>
                <asp:Button ID="Locate" runat="server" Text="Locate" onclick="Buttonuser_Click" />
                <br />
                <br />
            </td>
        </tr>
        </table>
        
    </td></tr>
    </table>
    
    <asp:Menu ID="Menu1" runat="server" onmenuitemclick="Menu1_MenuItemClick1" 
        Orientation="Horizontal" style="float: none">
        <Items>
            <asp:MenuItem ImageUrl="~/images/yumiselectedtab.gif" Text=" " Value="0">
            </asp:MenuItem>
            <asp:MenuItem ImageUrl="~/images/metromixunselectedtab.gif" Text=" " Value="1">
            </asp:MenuItem>
            <asp:MenuItem ImageUrl="~/images/dexknowsunselectedtab.gif" Text=" " Value="2">
            </asp:MenuItem>
            <asp:MenuItem ImageUrl="~/images/yelpunselectedtab.gif" Text=" " Value="3">
            </asp:MenuItem>
            <asp:MenuItem ImageUrl="~/images/chicagoreaderunselectedtab.gif" Text=" " Value="4">
            </asp:MenuItem>
            <asp:MenuItem ImageUrl="~/images/menuismunselectedtab.gif" Text=" " Value="5">
            </asp:MenuItem>
            <asp:MenuItem ImageUrl="~/images/menupagesunselectedtab.gif" Text=" " Value="6">
            </asp:MenuItem>
            <asp:MenuItem ImageUrl="~/images/yahoounselectedtab.gif" Text=" " Value="7">
            </asp:MenuItem>
            <asp:MenuItem ImageUrl="~/images/yellowpagesunselectedtab.gif" Text=" " 
                Value="8">
            </asp:MenuItem>
            <asp:MenuItem ImageUrl="~/images/citysearchunselectedtab.gif" Text=" " 
                Value="9">
            </asp:MenuItem>
        </Items>
    </asp:Menu>
    <asp:Label ID="Label1" runat="server"></asp:Label>
    <div id="Img">
        <asp:Image ID="divImg" runat="server" Height="20px" 
            ImageUrl="~/images/ajax-loader.gif" />
        <br />
    <cc1:GMap ID="GMap11" runat="server" Height="400px" 
                            Width="600px" Zoom="30"/>
    
    </div>
    <br />
    <br />
    
    <div style="height: 337px">
        <asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0"> 
            <asp:View ID="View1" runat="server">
                <table style="width:100%;">
                  <tr valign="top">
                    <td class="style5">
                      <table style="width:100%;">
                        <tr>
                          <td width="50%">
                            <asp:Table ID="Table1" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                          </td>
                        </tr>
                      </table> 
                    </td>
                    <td width="50">
                      <table style="width:100%;">
                      <tr>
                        <td valign="top">
                        <cc1:GMap ID="GMap1" runat="server" Height="400px" Visible="False" 
                            Width="600px" onclick="GMap2_Click" />
                        </td> 
                      </tr>
                      <tr>
                        <td>
                            <asp:Table ID="Table2" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                        </td>
                      </tr>
                      </table>
                    </td>
                  </tr>
                </table>        
            </asp:View>
            <asp:View ID="View2" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td rowspan="3" valign="top" width="50%">
                            <asp:Table ID="Table3" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                            </td>   
                   
                    <td valign="top" width="50%">
                        <cc1:GMap ID="GMap2" runat="server" Height="400px" Visible="False" 
                            Width="600px" onclick="GMap2_Click" />
                        </td>
                    </tr>
                    <tr><td></td></tr>    
                    <tr>
                        <td valign="top" width="50%">
                            <asp:Table ID="Table4" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                        </td>
                        </tr>
                </table>
            </asp:View>
            <asp:View ID="View3" runat="server">
                <table style="width:100%; height: 39px;">
                    <tr>
                        <td rowspan="3" valign="top" width="50%">
                            <asp:Table ID="Table5" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                        </td>
                        <td valign="top" width="50%">
                            
                            <cc1:GMap ID="GMap3" runat="server" Height="400px" Visible="False" 
                                Width="600px" />
                            
                        </td>    
                    </tr>
                    <tr><td></td></tr>
                    <tr>
                    <td valign="top" width="50%">
                    <asp:Table ID="Table6" runat="server" CssClass="tableStyle1">
                            </asp:Table></td>
                    </tr>
                </table>
            </asp:View>
            <asp:View ID="View4" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td rowspan="3" valign="top" width="50%">
                            <asp:Table ID="Table7" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                        </td>
                        <td valign="top" width="50%">
                            
                            <cc1:GMap ID="GMap4" runat="server" Height="400px" Visible="False" 
                                Width="600px" />
                            
                        </td>    
                    </tr>
                    <tr><td></td></tr>
                    <tr>
                        <td valign="top" width="50%">
                        <asp:Table ID="Table8" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                            </td>
                    </tr>
                    
                </table>
            </asp:View>
            <asp:View ID="View5" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td rowspan="3" valign="top" width="50%">
                            <asp:Table ID="Table9" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                        </td>
                        <td valign="top" width="50%">
                            
                            <cc1:GMap ID="GMap5" runat="server" Height="400px" Visible="False" 
                                Width="600px" />
                            
                        </td>    
                    </tr>
                    <tr><td></td></tr>
                    <tr>
                        <td valign="top" width="50%">
                        <asp:Table ID="Table10" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                            </td>
                    </tr>
                    
                </table>
            </asp:View>
            <asp:View ID="View6" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td rowspan="3" valign="top" width="50%">
                            <asp:Table ID="Table11" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                        </td>
                        <td valign="top" width="50%">
                            
                            <cc1:GMap ID="GMap6" runat="server" Height="400px" Visible="False" 
                                Width="600px" />
                            
                        </td>    
                    </tr>
                    <tr><td></td></tr>
                    <tr>
                        <td valign="top" width="50%">
                        <asp:Table ID="Table12" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                            </td>
                    </tr>
                    
                </table>
            </asp:View>
            <asp:View ID="View7" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td rowspan="3" valign="top" width="50%">
                            <asp:Table ID="Table13" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                        </td>
                        <td valign="top" width="50%">
                            
                            <cc1:GMap ID="GMap7" runat="server" Height="400px" Visible="False" 
                                Width="600px" />
                            
                        </td>    
                    </tr>
                    <tr><td></td></tr>
                    <tr>
                        <td valign="top" width="50%">
                        <asp:Table ID="Table14" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                            </td>
                    </tr>
                    
                </table>
            </asp:View>
            <asp:View ID="View8" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td rowspan="3" valign="top" width="50%">
                            <asp:Table ID="Table15" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                        </td>
                        <td valign="top" width="50%">
                            
                            <cc1:GMap ID="GMap8" runat="server" Height="400px" Visible="False" 
                                Width="600px" />
                            
                        </td>    
                    </tr>
                    <tr><td></td></tr>
                    <tr>
                        <td valign="top" width="50%">
                        <asp:Table ID="Table16" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                            </td>
                    </tr>
                    
                </table>
            </asp:View>
            <asp:View ID="View9" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td rowspan="3" valign="top" width="50%">
                            <asp:Table ID="Table17" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                        </td>
                        <td valign="top" width="50%">
                            
                            <cc1:GMap ID="GMap9" runat="server" Height="400px" Visible="False" 
                                Width="600px" />
                            
                        </td>    
                    </tr>
                    <tr><td></td></tr>
                    <tr>
                        <td valign="top" width="50%">
                        <asp:Table ID="Table18" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                            </td>
                    </tr>
                    
                </table>
            </asp:View>
            <asp:View ID="View10" runat="server">
                <table style="width:100%;">
                    <tr>
                        <td rowspan="3" valign="top" width="50%">
                            <asp:Table ID="Table19" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                        </td>
                        <td valign="top" width="50%">
                            
                            <cc1:GMap ID="GMap10" runat="server" Height="400px" Visible="False" 
                                Width="600px" />
                            
                        </td>    
                    </tr>
                    <tr><td></td></tr>
                    <tr>
                        <td valign="top" width="50%">
                        <asp:Table ID="Table20" runat="server" CssClass="tableStyle1">
                            </asp:Table>
                            </td>
                    </tr>
                    
                </table>
            </asp:View>
        </asp:MultiView>
    </div>
    </form> 
    <div id="novel"  
        
        
        
        
        
        
        
        style="position: absolute;display:none;top:243px; left:203px; width:327px; height:98px; text-align:left;" />
		Geocoder
		<form id="formForAddress" action="#" onsubmit="showAddress(this.address.value); return false">
        <table id="addTable">
        <tr>
            <td>StartingPoint</td>
            <td><input type="text" size="40" id="address" name="address" title="Type an address"/></td>
        </tr>
		<tr>
        <td colspan="2"><input type="submit" id="hae" value=" Submit " title="You can hit enter key as well"/></td>
        </tr>
		</table>
		</form>
</div>

<div>    
    </div>    
    <script type="text/javascript">
        _mPreferMetric = true;
        var map = new GMap2(document.getElementById("map"));
        
        var start = new GLatLng(37.4419,-122.1419);
        map.setCenter(start, 10);

        //pan and zoom to fit
        var bounds = new GLatLngBounds();

        function fit() {
            map.panTo(bounds.getCenter());
            map.setZoom(map.getBoundsZoomLevel(bounds));
        }


        ///Geo
        var geocoder = new GClientGeocoder();
        function showAddress(address) {
            geocoder.getLatLng(address, function (point) {
                if (!point) {
                    document.getElementById('address').style.color = 'red';
                    var marker = new GMarker(point);
                    map.addOverlay(marker);
                    document.getElementById('<%=TextBox2.ClientID%>').value = point;
                }
                else {
                    //draw(point);
                    map.panTo(point);
                    document.getElementById('address').style.color = 'black';
                    var marker = new GMarker(point);
                    map.addOverlay(marker);
                    document.getElementById('<%=TextBox2.ClientID%>').value = point;
                }
            })
        }
</script>         
</body>
</html>
