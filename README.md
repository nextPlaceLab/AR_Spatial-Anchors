# AR_Spatial-Anchors

![art Kopie](https://user-images.githubusercontent.com/57839896/174759248-b094959b-515b-4961-8253-821e1b03e09b.jpg)

## About

In diesem Prototyp wurde sogenannte Spacial Anchors als Tracking-Verfahren für 
das Anzeigen von Augmented-Reality-Inhalten erprobt. Dabei handelt es sich um 
einen von Microsoft bereitgestellten Entwickler:innendienst. Ein Vorteil dieses Verfahrens ist, dass räumlich verortete Inhalte gleichzeitig auf verschiedenen Devices 
synchronisiert angezeigt werden können und deren Position bzw. Transformation 
persistent gespeichert wird.
Als Entwicklungsplattform wurde wieder Unity3D verwendet. Die Anwendung ermöglicht das Setzen von Spacial Anchors (siehe Abbildung). Während dieses Vorgangs wird die Umgebung mittels Kameras und ggf. Tiefensensoren gescannt und 
eine digitale Punktwolke erstellt. Jedem Punkt in dieser Wolke wird dabei eine Position und ein Farbwert zugewiesen. Dadurch ergibt sich ein einzigartiger Schlüssel, der 
es ermöglicht, diese Umgebung und damit den Anchor später wieder identifizieren 
zu können. Durch das Speichern dieses Schlüssels in einer Cloud-Datenbank ist es 
nun möglich, von anderen AR Brillen oder Devices aus, den gleichen Ort zu identifizieren.
Nach dem Setzen des Anchors ist es auf diesem Wege möglich, Inhalte sowohl in der 
digitalen Anwendung als auch in der realen räumlichen Umgebung zu positionieren 
(siehe Abbildung). Zur genauen Ausrichtung der implementierten Inhalte und der 
damit verbundenen, dreidimensionalen Interaktion wird zudem ein weiteres, lokales Koordinatensystem, abhängig zur Position des Anchors erzeugt. Die AR-Brillen 
haben Zugriff auf eine serverseitige Datenbank, in der Informationen über die Spatial Anchors (u.A. deren Position oder zugewiesene Inhalte) gespeichert werden. Änderungen dieser Daten werden von der Datenbank automatisch an andere Clients 
verteilt und synchronisiert um die Inhalte auf mehreren Devices in gleicher Weise 
verortet darstellen zu können. Die Datenbank wurde dabei auf dem Projekt-Server 
aufgebaut. Die Anwendung wurde im Rahmen der AR-Kunstausstellung in der Essigfabrik erfolgreich getestet.

## Diagram

![Flowcharts für Publikation - 2 6  AR basierte Anwendung für Smartphone und Hololens mit Hilfe von Spatial Anchors Prototyp](https://user-images.githubusercontent.com/57839896/174759130-75cfd0ba-9451-45ca-b89d-681716d0171b.jpg)
