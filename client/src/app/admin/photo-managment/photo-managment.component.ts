import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_modules/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-managment',
  templateUrl: './photo-managment.component.html',
  styleUrls: ['./photo-managment.component.css'],
})
export class PhotoManagmentComponent implements OnInit {
  photos: Photo[] = [];

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.getPhotoForApprove();
  }

  getPhotoForApprove() {
    this.adminService.getPhotosForApproval().subscribe((photo) => {
      this.photos = photo;
    });
  }

  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe(() => {
      this.photos.splice(
        this.photos.findIndex((p) => p.id === photoId),
        1
      );
    });
  }

  rejectPhoto(photoId: number) {
    this.adminService.rejectPhoto(photoId).subscribe(() => {
      this.photos.splice(
        this.photos.findIndex((p) => p.id === photoId),
        1
      );
    });
  }
}
