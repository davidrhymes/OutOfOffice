document.addEventListener("DOMContentLoaded", function () {
    var managerSelect = document.getElementById("Select");
    var projectManagerID = document.getElementById("FKid");

    var selectedValue = managerSelect.value;
    projectManagerID.value = selectedValue;

    managerSelect.addEventListener("change", function () {
        selectedValue = managerSelect.value;
        projectManagerID.value = selectedValue;
    });
});