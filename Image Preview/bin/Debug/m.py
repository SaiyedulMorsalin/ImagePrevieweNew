import os
import sys
from PyQt5.QtWidgets import QApplication, QWidget, QPushButton, QVBoxLayout, QHBoxLayout, QLabel, QMenu, QMainWindow, QAction, QFileDialog, QMessageBox
from PyQt5.QtGui import QPixmap, QIcon
from PIL import Image
import shutil


class ImagePreview(QWidget):

    def __init__(self):
        super().__init__()

        self.Filepath = r"C:/Users/vn/Desktop"  # Default path for saving images
        self.saveThumbImages = r"C:/Newfolder"  # Save directory for small thumbnails
        self.extensions = ['jpg', 'png']  # Supported file extensions
        self._currentThumbSize = ThumbNailSize.Large  # Default size
        self.CurrentItem = None

        # Initialize the UI
        self.initUI()

    def initUI(self):

        # Create the layout and buttons
        layout = QVBoxLayout()

        self.iconButton1 = QPushButton('View Options')
        self.iconButton1.setIcon(QIcon('path_to_icon'))
        self.iconButton1.clicked.connect(self.showContextMenuView)
        layout.addWidget(self.iconButton1)

        self.iconButton3 = QPushButton('Sort Options')
        self.iconButton3.setIcon(QIcon('path_to_icon'))
        self.iconButton3.clicked.connect(self.showContextMenuSort)
        layout.addWidget(self.iconButton3)

        self.setLayout(layout)
        self.setWindowTitle('Image Preview')
        self.show()

    def showContextMenuView(self):
        # Context menu for changing thumbnail sizes
        menu = QMenu(self)

        tinyAction = QAction('Tiny', self)
        tinyAction.triggered.connect(lambda: self.changeThumbSize(ThumbNailSize.Tiny))
        menu.addAction(tinyAction)

        mediumAction = QAction('Medium', self)
        mediumAction.triggered.connect(lambda: self.changeThumbSize(ThumbNailSize.Medium))
        menu.addAction(mediumAction)

        largeAction = QAction('Large', self)
        largeAction.triggered.connect(lambda: self.changeThumbSize(ThumbNailSize.Large))
        menu.addAction(largeAction)

        menu.exec_(self.mapToGlobal(self.iconButton1.pos()))

    def showContextMenuSort(self):
        # Context menu for sorting
        menu = QMenu(self)

        oneStarAction = QAction('1 Star', self)
        oneStarAction.triggered.connect(lambda: self.sortByRating(1))
        menu.addAction(oneStarAction)

        twoStarAction = QAction('2 Stars', self)
        twoStarAction.triggered.connect(lambda: self.sortByRating(2))
        menu.addAction(twoStarAction)

        menu.exec_(self.mapToGlobal(self.iconButton3.pos()))

    def changeThumbSize(self, size):
        self._currentThumbSize = size
        self.reloadImages()  # Reload the images with the new size

    def reloadImages(self):
        # Populate the images with the updated thumbnail size
        self.populate(self.Filepath)

    def populate(self, path):
        if os.path.exists(path):
            self.loadDirectoryImages(path)

    def loadDirectoryImages(self, path):
        files = [f for f in os.listdir(path) if f.split('.')[-1] in self.extensions]
        for file in files:
            thumbnail = self.getThumbnail(os.path.join(path, file), self._currentThumbSize)
            # Here you would add logic to display the thumbnail in the UI (e.g., adding to a layout)

    def getThumbnail(self, image_path, size):
        img = Image.open(image_path)
        img.thumbnail((size, size))
        thumb_path = os.path.join(self.saveThumbImages, os.path.basename(image_path))
        img.save(thumb_path)
        return thumb_path  # Return the path of the thumbnail image

    def sortByRating(self, rating):
        # Implement sorting logic
        pass


class ThumbNailSize:
    Tiny = 70
    Medium = 100
    Large = 175


if __name__ == '__main__':
    app = QApplication(sys.argv)
    ex = ImagePreview()
    sys.exit(app.exec_())
