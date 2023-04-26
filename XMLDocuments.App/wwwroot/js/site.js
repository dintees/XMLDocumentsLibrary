document.addEventListener("DOMContentLoaded", () => {

    // XML creation
    // add event for changing checkbox
    document.getElementById("changeCreateFromFileSwitch").addEventListener("change", e => {
        const XMLTextarea = document.getElementById("XMLDocumentContent")
        const XMLFile = document.getElementById("XMLFile")
        XMLTextarea.disabled = !XMLTextarea.disabled
        XMLFile.disabled = !XMLFile.disabled
    })

})