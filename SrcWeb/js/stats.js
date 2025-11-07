async function setInfoMonster(monstre) {
  document.getElementById("monstre-nom").innerHTML = monstre.nom;
  document.getElementById("monstre-pv").innerHTML =
    monstre.pointsVieActuels + "/" + monstre.pointsVieMax;
  document.getElementById("monstre-force").innerHTML = monstre.force;
  document.getElementById("monstre-defense").innerHTML = monstre.defense;
  document.getElementById("monstre-niveau").innerHTML = monstre.niveau;
  document.getElementById("monstre-xp").innerHTML = monstre.experience;
  document.getElementById("monstre-type1").innerHTML = monstre.type1;
  if (monstre.type2)
    document.getElementById("monstre-type2").innerHTML = monstre.type2;
}

function clearInfoMonster() {
  document.getElementById("monstre-nom").innerHTML = "";
  document.getElementById("monstre-pv").innerHTML = "";
  document.getElementById("monstre-force").innerHTML = "";
  document.getElementById("monstre-defense").innerHTML = "";
  document.getElementById("monstre-niveau").innerHTML = "";
  document.getElementById("monstre-xp").innerHTML = "";
  document.getElementById("monstre-type1").innerHTML = "";
  document.getElementById("monstre-type2").innerHTML = "";
}