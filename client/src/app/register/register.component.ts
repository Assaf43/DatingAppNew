import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();

  model: any = {};

  constructor(
    private accountSercie: AccountService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {}

  register() {
    this.accountSercie.register(this.model).subscribe({
      next: () => {
        this.cancel();
      },
      error: (err) => {
        console.log(err);
        this.toastr.error(err.error.errors);
      },
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
