<script>
    window.flowchart = {
        initDrag: function () {
        console.log("Flowchart initialized");
    },

    saveToFile: function (json) {
        const blob = new Blob([json], {type: "application/json" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "flowchart.json";
    a.click();
    URL.revokeObjectURL(url);
    },

    loadFromFile: async function () {
        return new Promise((resolve) => {
            const input = document.createElement("input");
    input.type = "file";
    input.accept = ".json";
            input.onchange = (e) => {
                const file = e.target.files[0];
    const reader = new FileReader();
                reader.onload = (r) => resolve(r.target.result);
    reader.readAsText(file);
            };
    input.click();
        });
    }
};
</script>
