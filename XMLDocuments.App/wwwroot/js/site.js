document.addEventListener("DOMContentLoaded", () => {

    // XML creation
    // add event for changing checkbox
    if (document.getElementById("changeCreateFromFileSwitch")) {
        document.getElementById("changeCreateFromFileSwitch").addEventListener("change", e => {
            const XMLTextarea = document.getElementById("XMLDocumentContent")
            const XMLFile = document.getElementById("XMLFile")
            if (!e.target.checked) {
                XMLTextarea.disabled = false
                XMLFile.disabled = true
            } else {
                XMLTextarea.disabled = true
                XMLFile.disabled = false
            }

        })
    }

    // Selected function element
    const functionSelect = document.getElementById("SelectedFunction");
    if (functionSelect) {
        changeAttributesFieldsVisibility();
        functionSelect.addEventListener("change", changeAttributesFieldsVisibility);
    }

})

function changeAttributesFieldsVisibility() {
    const functionSelect = document.getElementById("SelectedFunction");
    const field1 = document.getElementById("AttributeName")
    const field2 = document.getElementById("AttributeValue")

    switch (functionSelect.value) {
        case "CheckNodeIfExists":
            field1.style.display = "none";
            field2.style.display = "none";
            break;
        case "GetNodeText":
            field1.style.display = "none";
            field2.style.display = "none";
            break;
        case "GetNodes":
            field1.style.display = "none";
            field2.style.display = "none";
            break;
        case "GetAllDocumentNodesQueries":
            field1.style.display = "none";
            field2.style.display = "none";
            break;
        case "GetAllDocumentNodesValues":
            field1.style.display = "none";
            field2.style.display = "none";
            break;
        case "GetAllAttributes":
            field1.style.display = "none";
            field2.style.display = "none";
            break;
        case "GetNodesWithAttribute":
            field1.style.display = "block";
            field2.style.display = "block";
            field1.setAttribute("placeholder", "Attribute name")
            field2.setAttribute("placeholder", "Attribute value (optional)")
            break;
        case "GetValueOfAttribute":
            field1.style.display = "block";
            field2.style.display = "none";
            field1.setAttribute("placeholder", "Attribute name")
            break;
        case "GetStructuredNodes":
            field1.style.display = "block";
            field2.style.display = "none";
            field1.setAttribute("placeholder", "Fields separated by \",\"")
            break;

        // ---

        case "AddNewNode":
            field1.style.display = "block";
            field2.style.display = "none";
            field1.setAttribute("placeholder", "XML node")
            break;
        case "AddAttributeToNode":
            field1.style.display = "block";
            field2.style.display = "block";
            field1.setAttribute("placeholder", "Attribute name")
            field1.setAttribute("placeholder", "Attribute value")
            break;
        case "RemoveAttributeFromNode":
            field1.style.display = "block";
            field2.style.display = "none";
            field1.setAttribute("placeholder", "Attribute name")
            break;
        case "DeleteNode":
            field1.style.display = "none";
            field2.style.display = "none";
            break;
        case "EditNodeText":
            field1.style.display = "block";
            field2.style.display = "none";
            field1.setAttribute("placeholder", "New text")
            break;
        case "EditNodeName":
            field1.style.display = "block";
            field2.style.display = "none";
            field1.setAttribute("placeholder", "New node name")
            break;
    }
}
