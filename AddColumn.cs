// With developer mode active, open the screen you want to customize. After the screen opens, go to Tools-> Customizations.
// WIth the screen that opens, go to the Scipt Editor tab and do the following :

// Ammend this class
public class Script{
  //Add the BAQDataView to the class
  BAQDataView baqDataView;
  // Ammend This Method
  public void CreateCustomView()
	{
    // Im using Time Phase Inquiry as an example here
    // Get the DataView that you want to add the columns to. In the customizations Properties tab,
    // you can find the referenced name of the dataview under EpiBinding
    EpiDataView timePhaseDataView = ((EpiDataView)(this.oTrans.EpiDataViews["TimePhas"]));
    // Then we get the table so we can modify it.
		DataTable timePhaseTable = timePhaseDataView.dataView.Table;
    
    // For each column we want to add, we create a DataColumn. 
		DataColumn projectID_Column = new DataColumn();
	    projectID_Column.DataType = System.Type.GetType("System.String"); // Type of the values the column holds
	    projectID_Column.ColumnName = "ProjectID"; // Name of the column, that will be displayed as the header of the column
	    projectID_Column.ExtendedProperties["ReadOnly"] = true; // Changes to column are not allowed
	    projectID_Column.ExtendedProperties["Enabled"] = false; // Column is enabled

    // Creating a second column
		DataColumn phase_Column = new DataColumn();
	    phase_Column.DataType = System.Type.GetType("System.String");
	    phase_Column.ColumnName = "Phase";
	    phase_Column.ExtendedProperties["ReadOnly"] = true;
	    phase_Column.ExtendedProperties["Enabled"] = false;

    // Then the columns will need to be added to the table
		timePhaseTable.Columns.Add(projectID_Column);
		timePhaseTable.Columns.Add(phase_Column);
    // Now if you where to reload, these columns should show up in the table you added them to, but with no data. If they dont show up, they may be marked hidden.
    // Hidden columns can be shown by going to the properties tab of the grid/table view, Under Layout, the Columns property, click on "(Collection"), on the far right of the
    // row a drop down arrow should now be shown, its easy to miss, click it, and the columns for the table/grid should be listed. Find the columns you want to show and make sure
    // hidden is not checked.
		
	}
	// Ammend This Method
	public void InitializeCustomCode()
	{
    // Make sure to add these calls so that the methods we add get called. If there is other code in the InitializeCustomCode section, leave it there.
    // These can just be added to the top of the method, as the first few lines.
    // Multiple BAQViews can be added.
		CreateBAQView();
		//Create"SomeName"BAQView();
		//Create"SomeOtherName"BAQView();
	}

  // Add method. Add as many of these as you have BAQDataViews that you want to add.
	public void CreateBAQView()
	{
    // baqDataView is whatever you named the baqDataView at the top of this class.
		baqDataView = new BAQDataView("Name of the BAQ you want to add");
    // I normally set the reference name of the baqDataView to be the same as the name of the BAQ, but you may not if you are calling
    // the same baq multiple times.
		oTrans.Add("Name that you want to call this baqDataView",baqDataView);

    // Pub Binding should be what updates/refreshes the data. Time Phase, for example, uses "Misc.PartNum" to determine what data to retrieve,
    // then uses that to populate the grid/table. This is also what the baq will be filtered by.
		string publisherBinding = "Misc.PartNum";
    // Creates a publisher, which will bind the baq and the "Misc.PartNum"
		IPublisher publisher = oTrans.GetPublisher(publisherBinding);
		if(pubJob==null)
		{	
      // Creates a generic unique name for the publisher. Publisher Names must be unique across epicor, as having multiple windows open with 
      // the same publisher name will cause issues. This is an easy way to prevent that from ever happening.
			string publisherName = Guid.NewGuid().ToString();
			oTrans.PublishColumnChange(publisherBinding, "publisherName");
			publisher = oTrans.GetPublisher(publisherBinding);
		}

		if(publisher !=null)
		{
      // Set the name of the column exactly as it shows on the baq. For example, if you wanted to match up Jobs by the part materials, it would be "JobMtl_PartNum"
			baqDataView.SubscribeToPublisher(publisher.PublishName, "Name of the column on the BAQ that is being filtered by the publisherBinding");
		}		
	}

  // The name of this method is important. The customization can help with creating this by going to the wizards tab,
  // selecting either Form Even Wizard, or Event Wizard, and adding a new handler. The generated method name is what you 
  // will want to use.
	private void TimePhas_AfterRowChange(EpiRowChangedArgs args)
	{
    // Get the Data View that we are modifying, the same one we added the columns too
		EpiDataView timePhaseDataView = ((EpiDataView)(this.oTrans.EpiDataViews["TimePhas"]));
    // Get its table, so that we can modify it.
		DataTable timePhaseTable = timePhaseDataView.dataView.Table;

    // Get the Data View of the baq that we created earlier. 
		EpiDataView baqDataView = ((EpiDataView)(this.oTrans.EpiDataViews["Name that you named the Data View(see Line 56)"]));
    // Get its table, so that we can retrieve data from it
		DataTable baqTable = baqDataView.dataView.Table;

    // Then a method of updating the data in the displayed table/grid will need to be added. The following is an example
    // of how I have it added to the Time Phase table/grid, but custom implications may be better suited for specific situtations.

    //**Note: This example references baqDataViews that are not shown in the above code. They would need to each be added above 
    // in order for the following example to work. This is just an example, and is meant to be modify to suit individual needs.

    // Loops through each row in the Time Phase table/grid
		foreach(DataRow row in timePhaseTable.Rows){
      // Checks to see if the row has a job number, indicating that the row references a job's supply/demand
			if((string)row["JobNum"] != ""){
        // Check each row in a baqDataView that I made for the job. 
				foreach(DataRow jobRow in jobHeadTable.Rows){
          // If that row's Job Number matches up with the Time Phase row Job Number
					if((string)row["JobNum"] == (string)jobRow["JobHead_JobNum"]){
            // set the ProjectID column that was made to the Job's ProjectID
						row["ProjectID"] = jobRow["JobHead_ProjectID"];
            // set the Phase column that was made to the Job's Phase
						row["Phase"] = jobRow["JobHead_PhaseID"];
						break;
					}
				}

        // Check each row in a baqDataView that I made for the job. This one is for the jobs materials. I had added a 2nd BAQ for the materials as
        // it was easier than making 1 BAQ that combinded the Job Part Number, and the Job Materials Part Numbers.
					foreach(DataRow jobRow in jobMaterialTable.Rows){
          // If that row's Job Number matches up with the Time Phase row Job Number
						if((string)row["JobNum"] == (string)jobRow["JobHead_JobNum"]){
            // set the ProjectID column that was made to the Job's ProjectID
							row["ProjectID"] = jobRow["JobHead_ProjectID"];
            // set the Phase column that was made to the Job's Phase
							row["Phase"] = jobRow["JobHead_PhaseID"];
              // We found our data, so we break in order to avoid doing more unnecessary loops
							break;
						}
					}
				
			}
      // Checks to see if the row has a order number, indicating that the row references a orders's supply/demand
			if((int)row["OrderNum"] != 0){

        // Check each row in a baqDataView that I made for the orders. 
				foreach(DataRow orderRow in orderTable.Rows){
          // If that row's Order Number matches up with the Time Phase row Order Number
					if((int)row["OrderNum"] == (int)orderRow["OrderHed_OrderNum"]){
            // set the ProjectID column that was made to the Order's ProjectID
						row["ProjectID"] = orderRow["OrderDtl_ProjectID"];
            // Checks to make sure the Order Line is also matched up to the selection. Our setup has an Order always having the projectID as one value, but each line 
            // being able to have its own Phase value. This could be added to the above if, but for some reason our data has some lines having blank projectID values
            // and this ensures, for us, that ProjectID will still get filled out if any line on the order has a ProjectID.
						if((int)row["OrderLine"] == (int)orderRow["OrderDtl_OrderLine"]){
              // set the Phase column that was made to the Order's Phase
							row["Phase"] = orderRow["OrderDtl_PhaseID_c"];
              // We found our data, so we break in order to avoid doing more unnecessary loops
							break;
						}
					}
				}
			}
    }
  }
}
