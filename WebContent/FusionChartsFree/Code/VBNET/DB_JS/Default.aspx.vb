﻿Imports DataConnection
Imports InfoSoftGlobal
Partial Class DB_JS_Default
    Inherits System.Web.UI.Page
    Public jsVarString As String
    Public indexCount As String
    Public Sub GetjsVar()

        'In this example, we show a combination of database + JavaScript rendering using FusionCharts.

        'The entire app (page) can be summarized as under. This app shows the break-down
        'of factory wise output generated. In a pie chart, we first show the sum of quantity
        'generated by each factory. These pie slices, when clicked would show detailed date-wise
        'output of that factory.

        'The XML data for the pie chart is fully created in ASP at run-time. ASP interacts
        'with the database and creates the XML for this.
        'Now, for the column chart (date-wise output report), we do not submit request to the server.
        'Instead we store the data for the factories in JavaScript arrays. These JavaScript
        'arrays are rendered by our ASP Code (at run-time). We also have a few defined JavaScript
        'functions which react to the click event of pie slice.

        'We've used an Access database which is present in ../DB/FactoryDB.mdb. 
        'It just contains two tables, which are linked to each other. 

        'Before the page is rendered, we need to connect to the database and get the
        'data, as we'll need to convert this data into JavaScript variables.

        'The following string will contain the JS Data and variables.
        'This string will be built in ASP and rendered at run-time as JavaScript.

        jsVarString = ""

        'Database Objects
        Dim oRs As DbConn, strQuery As String
        indexCount = 0

        'Iterate through each factory
        strQuery = "select * from Factory_Master"
        oRs = New DbConn(strQuery)

        While oRs.ReadData.Read()
            indexCount = indexCount + 1

            'Create JavaScript code to add sub-array to data array
            'data is an array defined in JavaScript (see below)
            'We've added vbTab & vbCRLF to data so that if you View Source of the
            'output HTML, it will appear properly. It helps during debugging
            jsVarString = jsVarString & vbTab & vbTab & "data[" & indexCount & "] = new Array();" & vbCrLf

            'Now create second recordset to get date-wise details for this factory
            strQuery = "select * from Factory_Output where FactoryId=" & oRs.ReadData("FactoryId").ToString() & " order by DatePro Asc" & vbCrLf
            Dim oRs2 As New DbConn(strQuery)
            While oRs2.ReadData.Read()
                'Put this data into JavaScript as another nested array.
                'Finally the array would look like data[factoryIndex][i][dataLabel,dataValue]
                jsVarString = jsVarString & vbTab & vbTab & "data[" & indexCount & "].push(new Array('" & Convert.ToDateTime(oRs2.ReadData("DatePro")).ToString("dd") & "/" & Convert.ToDateTime(oRs2.ReadData("DatePro")).ToString("MM") & "'," & oRs2.ReadData("Quantity").ToString() & "));" & vbCrLf

            End While
            'Close recordset
            oRs2.ReadData.Close()

        End While
        oRs.ReadData.Read()

    End Sub

    Public Function CreateChart() As String
        'Initialize the Pie chart with sum of production for each of the factories
        'strXML will be used to store the entire XML document generated
        Dim strXML As String, strQuery As String

        'Re-initialize Index
        indexCount = 0

        'Generate the graph element
        strXML = "<graph caption='Factory Output report' subCaption='By Quantity' decimalPrecision='0' showNames='1' numberSuffix=' Units' pieSliceDepth='20' formatNumberScale='0' >"

        'Iterate through each factory
        strQuery = "select * from Factory_Master"
        Dim oRs As New DbConn(strQuery)

        While oRs.ReadData.Read()
            'Update index count - sequential
            indexCount = indexCount + 1
            'Now create second recordset to get details for this factory
            strQuery = "select sum(Quantity) as TotOutput from Factory_Output where FactoryId=" & oRs.ReadData("FactoryId").ToString()
            Dim oRs2 As New DbConn(strQuery)
            oRs2.ReadData.Read()
            'Generate <set name='..' value='..' link='..' />
            'Note that we're setting link as updateChart(factoryIndex) - JS Function
            strXML = strXML & "<set name='" & oRs.ReadData("FactoryName").ToString() & "' value='" & oRs2.ReadData("TotOutput").ToString() & "' link='javascript:updateChart(" & indexCount & ")'/>"
            'Close recordset
            oRs2.ReadData.Close()
            oRs2 = Nothing
        End While
        'Finally, close <graph> element
        strXML = strXML & "</graph>"
        oRs.ReadData.Read()
        oRs = Nothing

        'Create the chart - Pie 3D Chart with data from strXML
        Return FusionCharts.RenderChart("../FusionCharts/FCF_Pie3D.swf", "", strXML, "FactorySum", "650", "300", False, True)

    End Function
    Public Function ShowNextChart() As String
        'Column 2D Chart with changed "No data to display" message
        'We initialize the chart with <graph></graph>
        Return FusionCharts.RenderChart("../FusionCharts/FCF_Column2D.swf?ChartNoDataText=Please click on a pie slice above to view detailed data.", "", "<graph></graph>", "FactoryDetailed", "600", "300", False, True)
    End Function
End Class