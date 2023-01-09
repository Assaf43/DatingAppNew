import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  NgxGalleryAnimation,
  NgxGalleryImage,
  NgxGalleryOptions,
} from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs';
import { User } from 'src/app/_models/user';
import { Member } from 'src/app/_modules/member';
import { Message } from 'src/app/_modules/message';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
  member: Member = {} as Member;
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];
  activeTab?: TabDirective;
  messages: Message[] = [];
  user?: User;

  constructor(
    private accountService: AccountService,
    private route: ActivatedRoute,
    private messageService: MessageService,
    public presenceService: PresenceService,
    private router: Router
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });
    this.router.routeReuseStrategy.shouldReuseRoute = () => false;
  }
  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  ngOnInit(): void {
    this.route.data.subscribe({
      next: (data) => (this.member = data['member']),
    });

    this.route.queryParams.subscribe({
      next: (params) => {
        params['tab'] && this.selectTab(params['tab']);
      },
    });

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false,
      },
    ];

    this.galleryImages = this.getImages();
  }

  getImages() {
    if (!this.member) return [];

    const imageUrl = [];
    for (const photo of this.member.photos) {
      imageUrl.push({
        small: photo.url,
        medium: photo.url,
        big: photo.url,
      });
    }

    return imageUrl;
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      this.memberTabs.tabs.find((x) => x.heading == heading)!.active = true;
    }
  }

  loadMessages() {
    if (this.member) {
      this.messageService.getMessageThread(this.member.userName).subscribe({
        next: (respone) => (this.messages = respone),
      });
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && this.user) {
      //this.loadMessages();
      this.messageService.createHubConnection(this.user, this.member.userName);
    } else {
      this.messageService.stopHubConnection();
    }
  }
}
